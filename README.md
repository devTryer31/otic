### Реализовано
- Сжатие папок и фалов с сохранением целостности структуры

### Формат файла .faac
1. [4] Сигнатура (4 bytes) = { 0xFA, 0xAA, 0xAA, 0xAC }
2. [4] Версия формата (1 int)
3. [1] Код сжатия с контекстом (1 byte)
4. [1] Код сжатия без контекста (1 byte)
5. [1] Код защиты от помех (1 byte)
6. [1] Резерв (1 byte)
7. [8] Указатель на начала данных файлов (1 long)
8. [4] Количество путей папок с данными (1 int)
9.  Folders data:  
    [4] Длина строки информации о папки в байтах UTF8 (1 int) = folderLen  
    [folderLen * 1] Информация о папке
10. [4] Количество файлов (1 int)
11. Files data:  
    [8] Указатель на начало информации о папке либо 0L, если файл лежит в корне (1 long)  
    [4] Количество байтов строки названия файла UTF8 (1 int) = len  
    [len * 1] Строка названия в байтах  
    [4] Длина файла в байтах (1 int) = fileLen  
    [fileLen * 1] Файл в байтах  