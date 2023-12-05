Wersja pythona użyta przy programowaniu: 3.11.4

Wymagane moduły: Pillow, numpy
Wersje modułów użyte podczas programowania:
   Pillow 10.1.0
   numpy 1.26.2

Aby użyć communicator.py należy skopiować "constants.template.py" do pliku o nazwie "constants.py" i edytować/zmienić odpowiednio potrzebne zmienne

Użycie:
python communicator.py [flagi]
gdzie python to plik wykonawczy pythona, a [flagi] to opcjonalnie jedna lub kilka z poniższych flag:
--filename lub -F: oznacza, że ścieżka do pliku (wejścia) podaje się w argumentach w wierszu poleceń. W przeciwnym przypadku program poprosi o podanie ścieżki po uruchomieniu
--send lub -S: Oznacza, że wyjście należy wysłać na serwer
--save lub -V: Oznacza, że wyjście należy zapisać do pliku
jeśli nie zostanie podana flaga --send lub flaga --save, to wyjście zostanie wypisane na standardowe wyjście (konsola)

Na wejściu znajduje się zdjęcie w orientacji poziomej (najlepiej 640x480) ze skalą po prawej stronie tak jak w obrazach w folderze ExampleImages
Na wyjściu znajduje się słownik zawierający procent pikseli które w obszarach roboczych zajmują gorące piksele oraz szacowaną największą temperaturę