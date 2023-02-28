# Parking detector
Детекция парковочных мест

## 1.Необходимые компоненты
- Cuda 11.6
- cuDNN 8.2.4 (Linux)/8.5.0.96 (Windows)
- ZLIB

## 2.Ошибки
Если вылетает ошибка 
>[ONNXRuntimeError] : 1 : FAIL : LoadLibrary failed with error 126 

Необходимо запускать собранный файл из `bin\debug`, по идее должно запуститься. Иначе множно раскомментировать создание обычной сессии и закомменитровать сессию с использованием Cuda
```
SessionOptions so = SessionOptions.MakeSessionOptionWithCudaProvider(0);
session = new InferenceSession(modelPath, so);
//session = new InferenceSession(modelPath);
```
