# Sistema de Turnos para Pacientes y Médicos

## Herramientas

* Visual Studio Code
  * Extensiones: C# for Visual Studio Code (Microsoft), C# Extensions (jchannon), NuGet Package Manager (jmrog).
* .NET Core SDK 3.1
* Git
* GitHub
* SQL Server
* SQL Server Management Studio (SSMS)

## Crear proyecto desde linea de comandos

Desde el cmd crear folder con el nombre del proyecto y entrar.

```bash
mkdir Turnos
cd Turnos
```

Estando dentro crear nuevo proyecto modelo-vista-controlador especificando la version de framework y el nombre del proyecto:

```dotnet
dotnet new mvc -f netcoreapp3.1 -o Turnos
```

Abrir el folder del proyecto con vscode.

Podemos ocultar de la vista algunos folders que solo se usan para guardar archivos cuando el proyecto se compila. File/Preferences/Settings/Text Editor/Files.

En la seccion exclude agregar `**/bin` y `**/obj.`

## Inicializar repo local y remoto

Segun lo visto en clase.

Podemos ayudarnos con [gitignore.io](https://www.toptal.com/developers/gitignore) para crear el archivo `.gitignore` que evita que subamos informacion innecesaria o sensible al repositorio remoto. En este caso podemos agregar palabras clave como VisualStudioCode, Windows, DotnetCore, ASPNETCore, Csharp, etc. Esto depende principalmente de las herramientas que estemos usando para desarrollar. Damos click en la opcion _crear_ y agregamos el contenido generado en el .gitignore de nuestro repo.

## Vista en localhost y otros

Para evitar problemas de certificados de seguridad al momento de correr el proyecto de forma local vamos a `Properties/launchSettings.json` y removemos la url https en `applicationUrl`, quedando solo la http. Luego en el archivo `Startup.cs` comentamos la linea `app.UseHttpsRedirection()`.

Entre los middleware que podemos encontrar en el archivo `Startup.cs` hay uno llamado `app.UseEndpoints` y es el encargado de establecer cual es el controlador por defecto y que vista va a abrir ese controlador (metodo Index del controller). El nombre del metodo es el mismo de la vista para que la pueda identificar.

Los archivos `.cshtml` permiten usar ASP.NET Razor, que a su vez facilita el uso de bloques de codigo (en este caso C#) dentro del HTML.

## Model

Primero vamos a crear el modelo de la tabla _especialidad_ y a traves de Entity Framework creamos automaticamente la base de datos mas adelante.

En la carpeta `Models` creamos un nuevo archivo `Especialidad.cs`. Las propiedades definidas aqui pasan a ser los campos de la tabla Especialidad en SQL Server. Indicamos la clave primaria con `[key]`.

Compilamos y ejecutamos el proyecto con

```dotnet
dotnet run
```

Asi podemos ver la app corriendo en el navegador en el puerto indicado.

## Controller

Es recomendado hacer primero algo sencillo que me permita comprobar que todas las capas de la aplicacion se estan conectando correctamente.

Los archivos de los controladores se deben nombrar con la vista a la que pertenecen + Cotroller para que puedan ser identificados. Ej: `EspecialidadController.cs`. Creamos el constructor (inicialmente vacio) y un metodo para mostrar la vista Index que vamos a crear.

**IActionResul**: muestra el retorno del metodo en la interfaz de usuario.

## View

En la carpeta `Views` creamos un nuevo folder `Especialidad` y dentro un archivo `Index.cshtml`.

Inicialmente creamos una tabla con dos especialidades para dejar la base de esta vista completa. Posteriormente se añadira el codigo para mostrar los registros de la bd.


`TurnosContext`: permite comunicarse con el Entity Framework y crear la base de datos a partir del codigo desarrollado. Esta clase hereda de `DbContext` y para poder usar esta ultima debemos agregar el package al `.csproj`. Para esto `Ctrl + P`, buscamos y seleccionamos _NuGet Package Manager: Add Package_, en el cuadro de busqueda que se abre buscamos _Microsoft.EntityFrameworkCore_ y luego seleccionamos la version requerida (en este caso 3.1.1).

Con esto la dependencia queda en el archivo `Turnos.csproj` y para actualizar el proyecto y usar la dependecia hacemos un restore:

```dotnet
dotnet restore
```

ya en la clase `TurnosContext` solo basta con referenciar el Entity Framework. Despues creamos el constructor

```dotnet
public TurnosContext(DbContextOptions<TurnosContext> opciones) : base(opciones)
```

Al constructor le pasamos un parametro `opciones` que es de tipo DbContextOptions y a su vez este ultimo recibe un tipo de dato TurnosContext. Con esto le pasamos al constructor opciones por defecto pero basandonos en la clase TurnosContext.

Para indicar que estas `opciones` que recibe son las que se usan por defecto (opciones base) usamos `: base(opciones)`, asi la clase TurnosContext hereda las opciones base.

Luego creamos la entidad (la tabla) que vamos a generar en SQL Server usando un `DbSet` de Entity Framework que va a ser de tipo Especialidad.

## appsettings.json

Ahora vamos a modificar este archivo de configuracion para establecer un string de conexion al motor de SQL Server. Agregamos lo siguiente al archivo:

```json
"ConnectionStrings": {
    "TurnosContext": "Data Source=.\\;Initial Catalog=Turnos;Persist Security Info=False;Trusted_Connection=True;"
  },
```

(Initial Catalog es el nombre con el que se va a crear la bd al ejecutar EF.)

Con esa configuracion inicial podemos conectarnos con las credenciales de Windows sin necesidad de dar ningun tipo de dato de autenticacion. Esto se hace para hacer pruebas pero mas adelante se debe cambiar por cuestiones de seguridad.

## Startup.cs

Editamos este archivo para referenciar la conexion a la base de datos que creamos en el archivo anterior. Vamos al metodo `ConfigureServices()` y agregamos a los servicios un metodo para establecer la conexion con la bd.

El metodo `DbContext` necesita dos partes importantes para funcionar; una es el tipo del contexto, en este caso es la clase `TurnosContext` donde estan definidas las tablas que van a formar parte de la DB. La otra parte es el motor de base de datos que va a usar (SqlServer) y su cadena de conexion (definida en appsettings.json).

Agregamos la dependencia _Microsoft.EntityFrameworkCore.SqlServer_, version 3.1.1 a nuestros proyecto (tal como se vio antes con NuGet Package).

`Configuration.GetConnectionString("TurnosContext")` con Configuration accedemos al appsettings.json y con getConnectionString accedemos a la propiedad TurnosContext que contiene la cadena de conexion. Agregar servicios al contenedor (Startup.cs) como acabamos de hacer se conoce como _"Inyeccion de Dependencias"_.

## Migrations de EF

Esta herramienta provista por Entity Framework (EF) nos va a ayudar a generar la estructura de la base de datos. Empezamos por abrir una terminal en vscode y corremos el comando:

```dotnet
dotnet tool install --global dotnet-ef --version 3.1
```

Esto instala Entity Framework en nuestra maquina de forma global (se puede usar tanto dentro como fuera del proyecto Turnos).

Si queremos instalarlo y usarlo solo de forma local al proyecto, estando en la raiz del mismo:

```dotnet
// La siguiente linea solo si el archivo no exiete
dotnet new tool-manifest

dotnet tool install dotnet-ef --version 3.1.1

// Lista las herramientas instaladas
dotnet tool list [--global]
```

Cuando el manifesto (`dotnet-tools.json`) ya existe, podemos instalar todas las herramientas listadas en el usamos:

```dotnet
dotnet tool restore
```

Para usar las herramientas:

```dotnet
// globales
dotnet-ef

// locales
dotnet dotnet-ef

// actualizar herramienta
dotnet tool update --global dotnet-ef // global
dotnet tool update dotnet-ef --version 3.1.1 // local

// desinstalar herramienta
dotnet tool uninstall --global dotnet-ef // global
dotnet tool uninstall dotnet-ef // local
```

## Agregar las referencias del framework instalado a los archivos de proyecto

Agregamos la dependencia _Microsoft.EntityFrameworkCore.Design_, Version 3.1.1 (la misma version de la herramienta instalada anteriormente) al proyecto para realizar las migraciones de los Modelos a la base de datos (herramienta Migrations).

Le damos un nombre al proceso inicial de migracion: especificamos que vamos a hacer una migracion de objetos usando el parametro `migration`, y con add le asignamos un nombre a dicha migracion inicial (`Migracion`).

```dotnet
dotnet dotnet-ef migrations add Migracion

// para deshacer la accion
ef migrations remove
```

## Instalar y configurar SQL Server y SSMS

Seguir las instrucciones de la seccion 2 de este [curso gratuito](https://www.udemy.com/course/introduccion-al-lenguaje-sql-server/?referralCode=A436394B71B77FC)

## Conexion con SQL Server

Abrimos SSMS, en server name ponemos dejamos la instancia por defecto `.\SQLEXPRESS` que fue la ruta que especificamos en el ConnectionString y en tipo de autenticacion dejamos `Windows Authentication` y nos conectamos.

Para confirmar el proceso de migracion inicial que habiamos iniciado anteriormente corremos:

```dotnet
dotnet dotnet-ef database update
```

Si volvemos a nuestra instancia de SQL Server y hacemos refresh, veremos que en el folder `Databases` ahora aparece la DB `Turnos`.

## Intregrar controlador y vista

Vamos al archivo `EspecialidadController.cs` y empezamos por completar el constructor para que reciba como parametro la clase `TurnosContext` y se pueda conectar con la DB.

Modificamos tambien el metodo Index para que la vista retorne todos los registros de la tabla Especialidad.

Luego actualizamos el archivo Views/Especialidad/Index.cshtml para agregar el modelo `Especialidad`. La forma en que se define permite iterar el modelo, recorrer los registros para leerlos y mostrarlos.

## Agregar registros a la tabla

En SSMS vamos a la DB `Turnos` y la tabla `Especialidad`, click derecho sobre ella y la opcion _Edit Top 200 Rows_ (o 1000) para poder insertar registros en la tabla. Creamos unas cuantas especialidades (el Id se pone de forma automatica) y luego en la terminal del proyecto ejecutamos `dotnet run`; abrimos el navegador y al final de la direccion ponemos el nombre del controlador para acceder a el (`http://localhost:5000/especialidad`) y poder ver los registros agregados.
