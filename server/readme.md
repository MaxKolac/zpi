Jak uruchomi� ten z�om:

 1. Komputer, kt�ry b�dzie serwerem musi doda� do swojej zapory ogniowej tak� zasad�:
    - Po��czenia przychodz�ce
    - Pozw�l na po��czenie
    - Profile: Wszystkie
    - Protok� TCP
    - Specyficzne porty: 25565, 25566, 25567
 2. Edytuj `exampleStart.bat` tak aby serwer by� uruchamiany na danym adresie IP i uruchom skrypt.
    Alternatywnie, serwer mo�na tez uruchomi� z konsoli:
     - `.\ZPIServer.exe` - uruchamia serwer na domy�lnym adresie IP \(127.0.0.1\).
     - `.\ZPIServer.exe 192.168.1.1` - uruchamia serwer na podanym adresie IP. Je�li adres jest z�y, uruchomi si� na adresie domy�lnym.