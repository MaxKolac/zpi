Jak uruchomiæ ten z³om:

 1. Komputer, który bêdzie serwerem musi dodaæ do swojej zapory ogniowej tak¹ zasadê:
    - Po³¹czenia przychodz¹ce
    - Pozwól na po³¹czenie
    - Profile: Wszystkie
    - Protokó³ TCP
    - Specyficzne porty: 25565, 25566, 25567
 2. Na serwerze musi byæ te¿ zainstalowany Python. <b>Podczas instalacji Python'a, opcja "Dodaj do PATH" musi byæ zaznaczona, inaczej serwer nie bêdzie w stanie wykryæ instalacji Python'a.</b>
 3. Edytuj `exampleStart.bat` tak aby serwer by³ uruchamiany na danym adresie IP i uruchom skrypt.
    Alternatywnie, serwer mo¿na tez uruchomiæ z konsoli:
     - `.\ZPIServer.exe` - uruchamia serwer na domyœlnym adresie IP \(127.0.0.1\).
     - `.\ZPIServer.exe 192.168.1.1` - uruchamia serwer na podanym adresie IP. Jeœli adres jest z³y, uruchomi siê na adresie domyœlnym.