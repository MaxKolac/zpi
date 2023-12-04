Jak uruchomić ten złom:
  1. Wybierasz, jaki rodzaj wiadomości ma zostać wysłany:
  - Gotowy `CameraDataMessage`, którego serwer już nie musi odkodowywać - czyli symulujesz kamerę zwykłą (testujesz `CameraSimulatorAPI`)
  - Zdjęcie JPG z termicznymi metadanymi, które serwer będzie próbował odczytać skryptem w Python'ie od FIlipa - czyli symulujesz kamerę Python (testujesz `PythonCameraSimulatorAPI`)
  2. W folderze, w którym znajduje się plik `.exe` znajdują się:
  - `message.json` - obiekt `CameraDataWithoutImage` zserializowany jako JSON z danymi z kamery. Będzie użyty przy budowaniu wiadomości w postaci obiektu `CameraDataMessage`.
  -	`imageToSend.png` - zdjęcie, które będzie wysłane na serwer <b>jako część `CameraDataMessage`. To nie będzie wysłane jako zdjęcie termiczne.</b>
  - `thermalImage.jpg` - zdjęcie termiczne, które będzie wysłane na serwer jako `byte[]`. Wybierz jedno ze zdjęć w folderze "ExampleImages", przenieś je do folderu z plikiem `.exe` i zmień jego nazwę na `thermalImage`.

  Dla wiadomości z "kamery zwykłej" użyta zostanie kombinacja `message.json` i `imageToSend.png`.

  Dla wiadomości z "kamery Python" użyte będzie tylko zdjęcie `thermalImage.jpg`.

  3. Uruchom program i podaj mu na jaki adres i port wysłać tak skonstruowaną wiadomość. Podanie pustego adresu IP ustawi adres na 127.0.0.1.

  Jeśli `message.json` nie istnieje lub nie udało się go zdeserializować do obiektu `CameraDataWithoutImage`, nowy plik JSON zostanie wygenerowany. Jeśli zdjęcia `imageToSend.png` nie odnaleziono, wysłana wiadomość nie będzie miała zdjęcia. 

  Program działa w pętli - nie trzeba go zamykać aby wysłać kolejną wiadomość. Po wysłaniu wiadomości, pliki `message.json` i `imageToSend.json` można je zmodyfikować przed ich ponownym załadowaniem do programu.