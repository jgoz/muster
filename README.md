# Muster
> [**muhs**-ter]  
> &mdash;_(verb)_ to assemble, as for orders, battle, etc.  
> &mdash;_(verb phrase)_ **muster in**, to enlist into service (in the armed forces).

Muster is a .NET (4.0) project aimed at simplifying most aspects of Windows Services &mdash; debugging, testing and deploying.

The core project, Muster, is a service abstraction layer and harnesses (console/service) based on a series of blog posts ([part one][p1], [part two][p2]) by [James Michael Hare][blog].

[p1]: http://geekswithblogs.net/BlackRabbitCoder/archive/2010/09/23/c-windows-services-1-of-2-creating-a-debuggable-windows.aspx 
[p2]: http://geekswithblogs.net/BlackRabbitCoder/archive/2010/10/07/c-windows-services-2-of-2-self-installing-windows-service-template.aspx 
[blog]: http://geekswithblogs.net/BlackRabbitCoder/Default.aspx 

The runner project, Muster.Runner or `muster-run.exe`, is a console application that can run or install services found in arbitrary assemblies.

Muster is [available on NuGet][nuget-pkg].

[nuget-pkg]: http://nuget.org/List/Packages/muster

*This project is currently considered unstable and may not suitable for production use.*

## Implementing services

All services must implement the `IWindowsService` interface. The interface defines 4 methods:

* `OnStart(String[] args)` &mdash; called when the service starts. **Must not block the calling thread.**
* `OnStop` &mdash; called when the service is stopped.
* `OnPause` &mdash; called when the service is paused. **Must not block the calling thread.**
* `OnContinue` &mdash; called when the service is resumed from being paused. **Must not block the calling thread.**
* `OnShutdown` &mdash; called when the operating system goes for shut down.

### Non-Blocking Methods
As noted above, the `OnStart`, `OnPause` and `OnContinue` methods must not block the calling thread. This is to ensure that the service is always able to receive Stop/Pause/Continue signals.

In the future, a service base class may be added to abstract the above threading requirements from the implementor.

### Example

```c#
[WindowsService("TestService",
    Description = "This is the description that will show up in the Windows Services management console.",
    EventLogSource = "TestServiceLogSource")]
public class TestService : IWindowsService
{
    public void OnStart(String[] args)
    {
        Console.WriteLine("We are starting up.");
    }

    public void OnStop()
    {
        Console.WriteLine("We are stopping.");
    }

    public void OnPause()
    {
        Console.WriteLine("We are pausing.");
    }

    public void OnContinue()
    {
        Console.WriteLine("We are resuming.");
    }

    public void OnShutdown()
    {
        Console.WriteLine("We are shutting down.");
    }

    public void Dispose()
    {
    }
}
```

## Using muster-run, the console runner
The Muster console runner enables `IWindowsService` implementations to be run from the console without needing to be installed, which can greatly simplify the development/testing cycle. The runner also provides a convenient way to install and uninstall services on the local machine.

Assuming `muster-run.exe` is on the system path and `MyService.dll` contains an `IWindowsService`, the following command is enough to get started:

    > muster-run MyService.dll

This will find and run all `IWindowsService` implementations in the given assembly. While running, the following prompt will be displayed:

    > [muster] Currently Running: [Q]uit [P]ause [R]esume

Pressing P will pause all running services, R will resume all paused services and Q will stop all services, exiting the runner.

### Usage options
    muster-run [OPTIONS]+ Assembly[,Assembly...]
    
    Options:
      -i, --install         Install the specified service(s)
      -u, --uninstall       Uninstall the specified service(s)
      -t, --types=VALUE     Comma-separated list of individual service types
      -c, --config=VALUE    Path to service configuration file
      -h, --help            Show this message and exit

#### Install
This will install the `IWindowsService`s in the given Assembly (or Assemblies) on the local machine.

#### Uninstall
This will uninstall the `IWindowsService`s contained in the given Assembly/Assmblies from the local machine. Note that the same assembly version should be used for installation and uninstallation.

#### Types
This allows specifying which services will actually run. The argument should be a comma-separated list of type names (full or partial) from any of the given assemblies.

    muster-run --types=ServiceOne,MyCompany.MyServices.ServiceTwo MyServices.dll

#### Config
This allows specifying a configuration file to be used as the `App.config` equivalent for all running services. All services must currently share the same configuration file.

## Compiling and packaging
To compile, run `build.cmd`. To create a NuGet package, run `package.cmd`.

This project, including the bundled Mono.Options source, is licenced under the [MIT licence][mit].

[mit]: http://www.opensource.org/licenses/mit-license.html

## TODO
* Service base class to abstract the threading requirements
* Use real logging strategy (Common.Logging?)
* Automatically reload AppDomain upon changes to target assemblies
* [WebActivator](https://bitbucket.org/davidebbo/webactivator/overview) harness
* <del>NuGet</del>
