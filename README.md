# Анализ данных Stack Exchange

Учебный проект по анализу данных сайта Stack Exchange.

## Основная цель

Этап обработки разбивается на две части:

1. Использовать автономный уровень [ADO.NET](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/) для загрузки данных, по разделам, в оперативную память и проведение первичной обработки. В результате должны получиться данные, которые можно визуализировать, для выявления интересных закономерностей или для дальнейшей обработки.

2. Использовать Python для визуализации данных. Модуль [multiprocessing](https://docs.python.org/3.6/library/multiprocessing.html) для распараллеливания процесса построения графиков. 

## Исходные данные

Данные доступны по [следующей ссылке.](https://archive.org/details/stackexchange)

## Требования для запуска 

* Часть, написанная на C#:
  * Visual Studio 2017.
  * .Net Framework 4.7.
* Часть, написанная на Python:
  * Версия Python 3.6 или выше.
  * Установленные пакеты [matplotlib](https://matplotlib.org/), [pandas](http://pandas.pydata.org/), [numpy](http://www.numpy.org/).

## Как запускать

1. [Предварительная обработка файлов на C#](/Src/CSharp/README.md).
2. [Построение графиков на Python](/Src/Python/README.md).
