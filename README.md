# Parking detector
Если вылетает ошибка 
>[ONNXRuntimeError] : 1 : FAIL : LoadLibrary failed with error 126 

<<<<<<< HEAD
Необходимо запускать собранный файл из `bin\debug`, по идее должно запуститься. Иначе множно раскомментировать создание обычной сессии и закомменитровать сессию с использованием Cuda
=======
Необходимо запускать собранный файл из `bin\debug`, по идее должно запуститься. Иначе множно раскоментировать создание обычной сессии и закоменитровать сессию с использованием Cuda
>>>>>>> 350b22f80f62df2fb3ae64c8254ac807d375b5bc
```
SessionOptions so = SessionOptions.MakeSessionOptionWithCudaProvider(0);
session = new InferenceSession(modelPath, so);
//session = new InferenceSession(modelPath);
```