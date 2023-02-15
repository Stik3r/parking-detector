# Parking detector
Если вылетает ошибка 
>[ONNXRuntimeError] : 1 : FAIL : LoadLibrary failed with error 126 

Необходимо запускать собранный файл из `bin\debug`, по идее должно запуститься. Иначе множно раскоментировать создание обычной сессии и закоменитровать сессию с использованием Cuda
```
SessionOptions so = SessionOptions.MakeSessionOptionWithCudaProvider(0);
session = new InferenceSession(modelPath, so);
//session = new InferenceSession(modelPath);
```