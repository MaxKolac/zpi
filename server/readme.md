# Dokumentacja

 ZPIServer to serwer napisany jako zwykła aplikacja konsolowa w języku C# i Python, która zbiera zdjęcia termiczne z kamer termowizyjnych wraz z ich metadanymi tj. odczyty temperatur i przekazuje te informacje do klientów w momencie odebrania od nich żądania.

 ## API - jak serwer obsługuje różne połączenia różnych urządzeń.

Serwer jako sama aplikacja z dostępem do sieci, do której mogą łączyć się różnie urządzenia, nigdy nie jest pewien jakie urządzenie próbuje się z nim połączyć i jakie dokładnie wyśle dane. 
    
Do tego dochodzi też problem, że różne modele kamer termowizyjnych mogą wysyłać różne dane, w różnych formatach, podczas gdy baza danych musi być jednolita.

Cały proces otrzymywania, odczytywania i odpowiadania na połączenia zawarty jest jako poniższa procedura, nazywana API:

![serwer-api-diagram](https://github.com/MaxKolac/zpi/assets/108830795/0d762aa3-c34e-4946-8a26-d61b79ac7931)

 - Na pierwszej linii w komunikacji między urządzeniami, a serwerem stoi klasa TcpReceiver. Jej zadaniem jest otrzymanie ciągu surowych bitów od nadawcy. Kiedy cała wiadomość zostanie otrzymana, TcpReceiver tworzy pakiet informacji TcpReceiverEventArgs. Tak utworzony pakiet jest przekazywany dalej poprzez inwokację zdarzenia OnSignalReceived. Klasa `TcpReceiverEventArgs` który zawiera:
   - Adres IP nadawcy
   - Port, z którego nadawca wysłał wiadomość
   - Pełną otrzymana wiadomość, jako ciąg bitów
 - Zadaniem klasy SignalTranslator jest rozpoznanie nadawcy i przekazanie otrzymanej wiadomości do odpowiedniego „tłumacza”. Kiedy otrzyma z tego zdarzenia pakiet informacji, pyta bazę danych o rekord z tabeli HostDevices, który posiada adres IP nadawcy. Z tego samego rekordu, odczytuje wartość kolumny Type. Na podstawie tej wartości SignalTranslator wie, gdzie otrzymane dane przekazać dalej. 
   - Jeśli nadawca został rozpoznany jako HostType.User, czyli użytkownik front-end, klasa SignalTranslator bierze na siebie odpowiedzialność przetłumaczenia jego żądania UserRequest.
 - W zależności od tego jakie urządzenie SignalTranslator rozpoznał, otrzymana wiadomość jest przekazywana do odpowiedniej biblioteki CameraAPI. Każdy model czy symulator kamery posiada własną bibliotekę, która tłumaczy różne formaty wiadomości na jeden wspólny format obiektu klasy CameraDataMessage.
 - Każdy komponent w serwerze może wysłać wiadomość zwrotną na podany adres IP i port poprzez klasę TcpSender. Jej zadaniem jest konwersja wiadomości na ciąg bitów i transmitowanie ich do podanego adresata. Nasłuchuje ona inwokacji zdarzeń w innych klasach. Te zdarzenia muszą przekazywać pakiet informacji o adresacie w postaci klasy `TcpSenderEventArgs`. Ta z kolei składa się z:
   - Adres IP adresata
   - Port adresata, na który wysłana będzie wiadomość
   - Pełna wiadomość zakodowana jako ciąg bitów

## Polecenia w konsoli - klasy `Logger` i `Command`

Jako aplikacja konsolowa, serwer może przyjmować dane wejściowe w formie poleceń. Pętla, która obsługuje wpisywane linijki z poleceniami znajduje się w głównej klasie serwera Program. 

Logger to klasa rozszerzająca standardową metodę Console.ReadLine() o poniższe funkcjonalności:
 -	Wypisywanie i logowanie informacji o tym co obecnie robi serwer
 -	Opcja pisania w różnych kolorach, w zależności od tego czy wiadomość jest ostrzeżeniem czy fatalnym błędem
 - Opcja dopisania prefiksu informującym o tym, który komponent serwera wypisał daną informację
 - Rozpoznawanie komend i ich argumentów oraz ich wykonywanie

Każde polecenie na serwerze posiada swoją własną klasę np.: PingCommand, która dziedziczy od klasy Command. Klasa Command wymaga implementacji od tych metod:
 -	SetArguments() – rozpoznaje i ustawia poprawnie podane argumenty polecenia
 -	GetHelp() – zwraca string, który zawiera instrukcję korzystania z danego polecenia
 -	Execute() – wykonuje dane polecenie. Każde wywołanie tej metody kończy się z inwokacją zdarzenia Command.OnExecuted

Na stan obecny, serwer rozpoznaje poniższe polecenia:
 - `db` - służy do obsługi bazy danych
 - `help` - wyświetla listę wszystkich poleceń lub instrukcję danego polecenia
 - `ping` - wysyła wiadomość testową na dany adres IP i port
 - `shutdown` - bezpiecznie kończy pracę serwera
 - `status` - wyświetla statystyki danego komponentu serwera

## Schemat bazy danych

Baza danych SQLite na serwerze składa się z dwóch tabel:

![database-scheme-background](https://github.com/MaxKolac/zpi/assets/108830795/5a191758-06d0-4253-9867-56f54f99e9d3)

### `HostDevices`

Tabela wszystkich urządzeń, które mogą połączyć się z serwerem. Dla wszystkich rekordów poniższe kolumny są obowiązkowe: 
 - `Name` - nazwa urządzenia w bazie danych; do celów organizacyjnych
 - `Type` - opisuje jakim urządzeniem jest dany rekord. Na podstawie tej kolumny serwer wie, jak obsłużyć żądania i połączenia z danego adresu IP. Wartością jest jeden z typów wyliczalnych HostDevice.HostType, który obecnie może przyjąć jedną z tych wartości:
   - `Unknown` - Urządzenie jest nieznane. Wartość nadawana dla połączeń odebranych od urządzeń, których adresu IP serwer nie znalazł w swojej bazie danych. Połączenia od tych urządzeń są automatycznie odrzucane
   - `CameraSimulator` - Urządzenie jest symulatorem kamery, czyli małą aplikacją konsolową, która wysyła gotową wiadomość CameraDataMessage, której serwer nie musi odkodowywać
   - `PuTTYClient` - Urządzenie jest aplikacją PuTTY. Używany kiedyś do testowania łączności. Jedyne co serwer robi z połączeniami od tych urządzeń to wypisuje otrzymane dane w konsoli jako tekst
   - `PythonCameraSimulator` - Urządzenie jest aplikacją front-end. Od tych urządzeń, serwer będzie się spodziewał że otrzyma wiadomości typu UserRequest, na podstawie których sformułuje odpowiednią wiadomość zwrotną
 - `Address` oraz `Port` - Jaki adres IP ma dane urządzenie. W połączeniu z kolumną Port, serwer wie na jaki adres i port ma wysłać informację zwrotną, tak aby została faktycznie odebrane.

Pozostałe kolumny są opcjonalne. Dla rekordów reprezentujących różne modele kamer, zaleca się aby te kolumny nie były puste:
 - `LocationAltitude`, `LocationLatitude` oraz `LocationDescription` – po kolei szerokość i wysokość geograficzna oraz zwięzły opis gdzie dane urządzenie zostało zamontowane (na drzewie, na słupie itd.)
 - `SectorId` - klucz obcy do tabeli Sectors, określa do którego z sektorów dane urządzenie jest przypisane`
 - `LastDeviceStatus` - ostatni znany stan urządzenia. Wartością jest typ wyliczalny HostDevice.DeviceStatus. Ten typ wyliczalny ma wartości podzielone na trzy grupy, w zależności od tego czy urządzenie jest sprawne (0-99), wymaga uwagi (100-199) czy (200-299) niesprawne:
   - `Unknown (0)` - urządzenie jest nieznane lub go nie rozpoznano
   - `OK (1)` - urządzenie jest sprawne
   - `LowPower (100)` - urządzenie zgłasza niski poziom naładowania i/lub nie ma zasilania
   - `Disconnected (200)` - nie udało się nawiązać się połączenia z tym urządzeniem
   - `Unresponsive (201)` - urządzenie jest widoczne z poziomu sieci, ale nie odpowiada na żadne żądania
   - `DataCorrupted (202)` - ostatnia otrzymana wiadomość od urządzenia była nieczytelna
 - `LastFireStatus` - ostatni znany poziom zagrożenia pożarowego. Wartością jest typ wyliczalny HostDevice.FireStatus, gdzie:
   - `OK` - oznacza brak pożaru
   - `Suspected` - oznacza podejrzenie o możliwym pożarze; wymagane jest potwierdzenie po stronie klienta front-end
   - `Confirmed` - oznacza potwierdzenie pożaru przez klienta front-end

### `Sectors`

Druga tabela jest o wiele mniejsza – zawiera rekordy opisujące każdy sektor, na które Las Kabacki został podzielony:
 - 'Name' - nazwa sektora
 - 'Description' - opcjonalny opis sektora

# Jak uruchomić ten złom:

 1. Komputer, który będzie serwerem musi dodać do swojej zapory ogniowej taką zasadę:
    - Połączenia przychodzące
    - Pozwól na połączenie
    - Profile: Wszystkie
    - Protokół TCP
    - Specyficzne porty: 25565, 25566, 25567
 2. Na serwerze musi być też zainstalowany Python. <b>Podczas instalacji Python'a, opcja "Dodaj do PATH" musi być zaznaczona, inaczej serwer nie będzie w stanie wykryć instalacji Python'a.</b>
 3. Edytuj `exampleStart.bat` tak aby serwer był uruchamiany na danym adresie IP i uruchom skrypt.
    Alternatywnie, serwer można tez uruchomić z konsoli:
     - `.\ZPIServer.exe` - uruchamia serwer na domyślnym adresie IP \(127.0.0.1\).
     - `.\ZPIServer.exe 192.168.1.1` - uruchamia serwer na podanym adresie IP. Jeśli adres jest zły, uruchomi się na adresie domyślnym.
