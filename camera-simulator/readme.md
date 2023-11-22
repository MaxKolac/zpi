Jak uruchomić ten złom:
  1. W folderze, w którym znajduje się plik `.exe` znajdują się:
  - `message.json` - obiekt `CameraDataMessage` zserializowany jako JSON z danymi z kamery.
  -	`imageToSend.png` - zdjęcie, które będzie wysłane na serwer.
  2. W `message.json` edytuj wartości, które nie są zdjęciem `Image`. Obrazek `imageToSend.png` edytuj jak chcesz, ale nie usuwaj.
  3. Uruchom program i podaj mu na jaki adres i port wysłać tak skonstruowaną wiadomość. Podanie pustego adresu IP ustawi adres na 127.0.0.1.

  Jeśli `message.json` nie istnieje lub nie udało się go zdeserializować do obiektu `CameraDataMessage`, nowy plik JSON zostanie wygenerowany. Jeśli zdjęcia `imageToSend.png` nie odnaleziono, wysłana wiadomość nie będzie miała zdjęcia. 

  Program działa w pętli - nie trzeba go zamykać aby wysłać kolejną wiadomość. Po wysłaniu wiadomości, pliki `message.json` i `imageToSend.json` można zmodyfikować przed ich ponownym załadowaniem do programu.