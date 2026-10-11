# SIUS-Client Schnittstellendokumentation

Diese Datei beschreibt die aktuell implementierte Schnittstelle zwischen dem Windows-Client (`client/Form1.cs`) und der Webapplikation.

Die Dokumentation ist bewusst auf die Endpunkte beschränkt, die der Client wirklich nutzt. Es gibt aktuell keine generische CRUD-API für Anlässe, Schützen oder Schussdaten.

## Überblick

- **Client**: Windows Forms Anwendung in C#.
- **Server**: PHP-Webapplikation mit JSON-API unter `/api/...`.
- **Transport**: HTTP oder HTTPS.
- **Datenformat**: JSON für API-Aufrufe, CSV für den lokalen SIUS-Dateiaustausch.
- **Authentifizierung**: Bearer Token im HTTP-Header `Authorization`.
- **Polling**: Der Client prüft alle 3 Sekunden die Importdatei und neue Schützen.

## Ablauf im Client

1. Benutzer gibt Server-Adresse, Benutzername, Passwort, Importdatei und Exportdatei ein.
2. Client meldet sich mit `POST /api/login` am Server an.
3. Server liefert ein Bearer Token.
4. Client setzt `Authorization: Bearer <token>` für alle weiteren API-Aufrufe.
5. Client lädt alle Anlässe über `GET /api/anlaesse`.
6. Benutzer wählt einen Anlass.
7. Client exportiert die Schützen dieses Anlasses in die lokale Export-CSV.
8. Hintergrundprozess startet:
   - neue SIUS-Schusszeilen aus der Import-CSV lesen,
   - Schüsse an den Server senden,
   - neue Schützen vom Server holen und an die Export-CSV anhängen.

## Basis-URL

Die Server-Adresse wird im Client eingegeben, zum Beispiel:

```text
https://example.ch
http://localhost
```

Die Adresse muss mit `http://` oder `https://` beginnen. Der Client entfernt ein abschließendes `/` automatisch.

## Authentifizierung

### POST `/api/login`

Meldet den Client an und gibt ein API-Token zurück.

Dieser Endpunkt benötigt noch keinen Bearer Token.

**Request**

```json
{
  "Username": "admin",
  "Password": "secret"
}
```

Der Server akzeptiert auch kleingeschriebene Feldnamen:

```json
{
  "username": "admin",
  "password": "secret"
}
```

**Response `200 OK`**

```json
{
  "token": "base64urlPayload.hmacSignature",
  "expiresIn": 604800
}
```

`expiresIn` ist die Gültigkeit in Sekunden. Aktuell sind das 7 Tage.

**Fehler `400 Bad Request`**

```json
{
  "success": false,
  "message": "Benutzername und Passwort sind erforderlich.",
  "errorCode": "INVALID_INPUT"
}
```

**Fehler `401 Unauthorized`**

```json
{
  "success": false,
  "message": "Ungueltige Anmeldedaten.",
  "errorCode": "INVALID_CREDENTIALS"
}
```

## Header für geschützte Endpunkte

Alle folgenden Endpunkte benötigen:

```http
Authorization: Bearer <token>
Accept: application/json
```

JSON-Requests verwenden zusätzlich:

```http
Content-Type: application/json; charset=utf-8
```

Wenn der Token fehlt, ungültig oder abgelaufen ist, antwortet der Server mit:

```json
{
  "success": false,
  "message": "Authentifizierung erforderlich.",
  "errorCode": "UNAUTHORIZED"
}
```

## Anlässe

### GET `/api/anlaesse`

Lädt alle Anlässe für die Auswahl im Client.

**Response `200 OK`**

```json
[
  {
    "id": 1,
    "name": "Feldschiessen 2026"
  }
]
```

**Felder**

| Feld | Typ | Beschreibung |
| --- | --- | --- |
| `id` | int | ID des Anlasses aus der Datenbank. |
| `name` | string | Anzeigename des Anlasses. |

## Schützenexport

### GET `/api/anlaesse/{id}/shooters`

Lädt alle Schützen beziehungsweise Standblätter eines Anlasses. Der Client schreibt daraus die lokale Export-CSV für SIUS.

**Pfadparameter**

| Parameter | Typ | Beschreibung |
| --- | --- | --- |
| `id` | int | Anlass-ID. |

**Response `200 OK`**

```json
[
  {
    "id": 12,
    "startnummer": 12,
    "name": "Muster",
    "vorname": "Max",
    "verein": "Schützenverein Beispiel",
    "bahn": 0,
    "abloesung": 0,
    "aktiv": true
  }
]
```

**Felder**

| Feld | Typ | Beschreibung |
| --- | --- | --- |
| `id` | int | ID des Standblatts. |
| `startnummer` | int | Startnummer für SIUS. Aktuell identisch mit der Standblatt-ID. |
| `name` | string | Nachname. |
| `vorname` | string | Vorname. |
| `verein` | string | Verein/Zusatz/Firmenanrede aus der Adresse. |
| `bahn` | int | Aktuell immer `0`. |
| `abloesung` | int | Aktuell immer `0`. |
| `aktiv` | bool | Aktuell immer `true`. |

**Fehler `400 Bad Request`**

```json
{
  "success": false,
  "message": "Ungueltiger Anlass.",
  "errorCode": "INVALID_ANLASS"
}
```

## Neue Schützen

### GET `/api/anlaesse/{id}/shooters/new?sinceId={lastId}`

Lädt nur Schützen/Standblätter, deren ID größer als `sinceId` ist. Der Client nutzt diesen Endpunkt im Hintergrund und hängt neue Zeilen an die Export-CSV an.

**Pfadparameter**

| Parameter | Typ | Beschreibung |
| --- | --- | --- |
| `id` | int | Anlass-ID. |

**Queryparameter**

| Parameter | Typ | Beschreibung |
| --- | --- | --- |
| `sinceId` | int | Letzte bereits bekannte Standblatt-ID. Fehlt der Parameter, wird `0` verwendet. |

**Response `200 OK`**

```json
[
  {
    "id": 13,
    "startnummer": 13,
    "name": "Beispiel",
    "vorname": "Erika",
    "verein": "",
    "bahn": 0,
    "abloesung": 0,
    "aktiv": true
  }
]
```

Bei keinen neuen Schützen wird ein leeres Array zurückgegeben:

```json
[]
```

## Schussimport

### POST `/api/anlaesse/{id}/shots/import`

Importiert eine Liste von SIUS-Schussdatensätzen für einen Anlass.

Der Client sendet nur neu erkannte Schüsse. Duplikate werden clientseitig anhand von `LogEvent` und der kompletten CSV-Zeile reduziert. Serverseitig werden die empfangenen Schüsse ohne zusätzliche Duplikatprüfung eingefügt.

**Pfadparameter**

| Parameter | Typ | Beschreibung |
| --- | --- | --- |
| `id` | int | Anlass-ID. |

**Request**

```json
{
  "AnlassId": 1,
  "Shots": [
    {
      "StartNr": 12,
      "Primaerwertung": 10.4,
      "Schussart": 0,
      "BahnNr": 3,
      "Sekundaerwertung": 10,
      "Teiler": 123,
      "Zeit": "2026-04-28 14:30:15",
      "Mouche": 1,
      "X": 0.12,
      "Y": -0.34,
      "InTime": 1,
      "TimeSinceChange": 2.5,
      "SweepDirection": 0,
      "Demonstration": 0,
      "Match": 1,
      "Stich": 1,
      "InsDel": 0,
      "TotalArt": 0,
      "Gruppe": 0,
      "Feuerart": 0,
      "LogEvent": 123456,
      "LogTyp": 1,
      "ZeitSeitJahresbeginn": 102030,
      "Abloesung": 0,
      "Waffe": 0,
      "Position": 0,
      "TargetId": 3,
      "ExterneNummer": 0
    }
  ]
}
```

Der Server akzeptiert für `AnlassId` und `Shots` auch `anlassId` und `shots`. Innerhalb eines Schusses akzeptiert der Server die gezeigten PascalCase-Feldnamen und zusätzlich die Variante mit kleinem Anfangsbuchstaben, zum Beispiel `startNr`.

**Response `201 Created`**

```json
{
  "success": true,
  "imported": 1
}
```

`imported` enthält die Anzahl erfolgreich eingefügter Datensätze.

**Fehler `400 Bad Request`: Anlass passt nicht**

```json
{
  "success": false,
  "message": "Ungueltiger Anlass.",
  "errorCode": "INVALID_ANLASS"
}
```

Dieser Fehler tritt auf, wenn die Anlass-ID im Pfad ungültig ist oder `AnlassId` im Body gesetzt ist und nicht zur Pfad-ID passt.

**Fehler `400 Bad Request`: Shots ist kein Array**

```json
{
  "success": false,
  "message": "Shots muss ein Array sein.",
  "errorCode": "INVALID_INPUT"
}
```

**Fehler `500 Internal Server Error`**

```json
{
  "success": false,
  "message": "Schussdaten konnten nicht importiert werden.",
  "errorCode": "IMPORT_FAILED"
}
```

## SIUS-CSV Importdatei

Der Client liest die Importdatei mit `ISO-8859-1` und erwartet Semikolon als Trennzeichen.

Eine gültige Schusszeile muss mindestens 28 Felder haben. Zusätzliche Felder werden ignoriert.

**Reihenfolge der Felder**

| Position | JSON-Feld | Typ |
| --- | --- | --- |
| 1 | `StartNr` | int |
| 2 | `Primaerwertung` | decimal |
| 3 | `Schussart` | int |
| 4 | `BahnNr` | int |
| 5 | `Sekundaerwertung` | decimal |
| 6 | `Teiler` | int |
| 7 | `Zeit` | string |
| 8 | `Mouche` | int |
| 9 | `X` | decimal |
| 10 | `Y` | decimal |
| 11 | `InTime` | int |
| 12 | `TimeSinceChange` | decimal |
| 13 | `SweepDirection` | int |
| 14 | `Demonstration` | int |
| 15 | `Match` | int |
| 16 | `Stich` | int |
| 17 | `InsDel` | int |
| 18 | `TotalArt` | int |
| 19 | `Gruppe` | int |
| 20 | `Feuerart` | int |
| 21 | `LogEvent` | long |
| 22 | `LogTyp` | int |
| 23 | `ZeitSeitJahresbeginn` | long |
| 24 | `Abloesung` | int |
| 25 | `Waffe` | int |
| 26 | `Position` | int |
| 27 | `TargetId` | int |
| 28 | `ExterneNummer` | int |

Dezimalwerte dürfen in der CSV ein Komma oder einen Punkt verwenden. Der Client normalisiert Kommas vor dem JSON-Versand zu Punkten.

## SIUS-CSV Exportdatei

Der Client schreibt beim Auswählen eines Anlasses die komplette Schützenliste mit `ISO-8859-1`, ohne Header und mit Komma als Trennzeichen. Jede Zeile hat elf Felder: ID, Name, Vorname, Anzeigename und sieben leere Felder. Die erste Zeile ist stets der Dummy-Eintrag mit ID 0.

**Beispiel**

```csv
0,DummyName,Dummy,D. DummyName,,,,,,,
12,Muster,Max,M. Muster,,,,,,,
```

Textfelder mit Kommas, Anführungszeichen oder Zeilenumbrüchen werden in Anführungszeichen eingeschlossen; enthaltene Anführungszeichen werden verdoppelt. Bei leerem Vornamen besteht der Anzeigename nur aus dem Nachnamen. Zeichen außerhalb von ISO-8859-1 führen zu einem sichtbaren Exportfehler statt stiller Ersetzung durch Fragezeichen.

Neue Schützen werden anhand ihrer ID dedupliziert und ergänzt. Vollständiger Export und Ergänzungen werden zunächst in eine temporäre Datei im selben Verzeichnis geschrieben und dann atomar ausgetauscht. Bei Schreibfehlern bleibt die bisherige Datei erhalten. Dateisperren werden bis zu fünfmal mit asynchroner Wartezeit behandelt.

## Fehlerverhalten und Importfortschritt

- Der Import liest ab der zuletzt bestätigten Byteposition, höchstens 64 KiB bzw. 500 vollständige Zeilen pro Durchlauf. Eine Zeile wird erst nach ihrem abschließenden LF (oder CRLF) verarbeitet.
- Kürzere Dateien werden wieder ab Position 0 gelesen; bereits bestätigte LogEvent-IDs bleiben dabei dedupliziert. Bei Austausch oder Überschreiben durch eine gleich große oder größere Datei muss „Neu einlesen“ verwendet werden.
- Fortschritt, Duplikatmarkierungen und Zähler werden erst nach HTTP-Erfolg und `success: true` im JSON gespeichert. Bei einem Übertragungsfehler wird der Block erneut versucht.
- Ungültige vollständige Zeilen stoppen den betreffenden Block mit einer Fehlermeldung in der Fenstertitelleiste. Nach Korrektur wird automatisch erneut gelesen. Sie werden nicht stillschweigend verworfen.
- Import- und Exportfehler werden unabhängig behandelt und in der Fenstertitelleiste angezeigt; nach erfolgreichem Wiederholungsversuch wird die jeweilige Meldung entfernt. HTTP-Anfragen haben ein Timeout von 30 Sekunden.
- Anlasswechsel, erneutes Verbinden und „Neu einlesen“ warten auf das Ende der bisherigen Überwachung. Beim Anlasswechsel wird der Importfortschritt zurückgesetzt. Während der Überwachung sind Pfade und Verbindungsdaten gegen Änderungen gesperrt.
- Der Fortschritt wird nur im Arbeitsspeicher gehalten. Nach Neustart bzw. „Neu einlesen“ wird die Datei erneut verarbeitet. Falls der Server Daten speichert, aber seine Antwort verloren geht, kann auch eine Wiederholung Duplikate erzeugen. Für garantierte Duplikatfreiheit muss die Server-API Importe idempotent behandeln; dies wird durch den Client allein nicht garantiert.

## Lokale Regressionstests

`powershell -File tests/run.ps1` baut den Debug-Client ohne ClickOnce-Signierung und führt die Regressionstests aus. Benötigt werden Visual Studio mit MSBuild, .NET Framework 4.7.2 und die bereits wiederhergestellten NuGet-Pakete. Die Tests verwenden temporäre Dateien und einen simulierten HTTP-Serverhandler, keine echte Serververbindung.

## Datenzuordnung auf dem Server

Beim Schussimport werden die JSON-Felder in die Tabelle `schussdaten` übertragen.

| JSON-Feld | Datenbankfeld |
| --- | --- |
| `AnlassId` oder Pfad-ID | `id_anlass` |
| `StartNr` | `start_nr` |
| `Primaerwertung` | `primaerwertung` |
| `Schussart` | `schussart` |
| `BahnNr` | `bahn_nr` |
| `Sekundaerwertung` | `sekundaerwertung` |
| `Teiler` | `teiler` |
| `Zeit` | `schuss_zeit` |
| `Mouche` | `mouche` |
| `X` | `x_koordinate` |
| `Y` | `y_koordinate` |
| `InTime` | `in_time` |
| `TimeSinceChange` | `time_since_change` |
| `SweepDirection` | `sweep_direction` |
| `Demonstration` | `demonstration` |
| `Match` | `match_index` |
| `Stich` | `stich_index` |
| `InsDel` | `ins_del` |
| `TotalArt` | `total_art` |
| `Gruppe` | `gruppe` |
| `Feuerart` | `feuerart` |
| `LogEvent` | `log_event` |
| `LogTyp` | `log_typ` |
| `ZeitSeitJahresbeginn` | `zeit_seit_jahresanfang` |
| `Abloesung` | `abloesung` |
| `Waffe` | `waffe` |
| `Position` | `position` |
| `TargetId` | `target_id` |
| `ExterneNummer` | `externe_nummer` |

`created_by_user_id` und `updated_by_user_id` werden aus dem angemeldeten API-Benutzer gesetzt.

## Implementierte Routen

Aktuell sind für den SIUS-Client diese API-Routen registriert:

| Methode | Route | Zweck |
| --- | --- | --- |
| POST | `/api/login` | Login und Tokenausgabe |
| GET | `/api/anlaesse` | Anlassliste |
| GET | `/api/anlaesse/{id}/shooters` | Alle Schützen eines Anlasses |
| GET | `/api/anlaesse/{id}/shooters/new?sinceId=...` | Neue Schützen seit letzter ID |
| POST | `/api/anlaesse/{id}/shots/import` | Schussdaten importieren |

Nicht implementiert sind zum Beispiel:

- `POST /api/auth/login`
- `GET /api/anlass`
- `POST /api/anlass`
- `GET /api/schuetze`
- `GET /api/schussdaten`
- `PUT`- oder `DELETE`-Endpunkte für diese API

## Beispiel mit curl

### Login

```bash
curl -X POST http://localhost/api/login \
  -H "Content-Type: application/json" \
  -d '{"Username":"admin","Password":"secret"}'
```

### Anlässe laden

```bash
curl http://localhost/api/anlaesse \
  -H "Authorization: Bearer TOKEN"
```

### Schützen laden

```bash
curl http://localhost/api/anlaesse/1/shooters \
  -H "Authorization: Bearer TOKEN"
```

### Schüsse importieren

```bash
curl -X POST http://localhost/api/anlaesse/1/shots/import \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"AnlassId":1,"Shots":[{"StartNr":12,"Primaerwertung":10.4,"Schussart":0,"BahnNr":3,"Sekundaerwertung":10,"Teiler":123,"Zeit":"2026-04-28 14:30:15","Mouche":1,"X":0.12,"Y":-0.34,"InTime":1,"TimeSinceChange":2.5,"SweepDirection":0,"Demonstration":0,"Match":1,"Stich":1,"InsDel":0,"TotalArt":0,"Gruppe":0,"Feuerart":0,"LogEvent":123456,"LogTyp":1,"ZeitSeitJahresbeginn":102030,"Abloesung":0,"Waffe":0,"Position":0,"TargetId":3,"ExterneNummer":0}]}'
```
