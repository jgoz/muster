# Muster
> [**muhs**-ter]  
> &mdash;_(verb)_ to assemble, as for orders, battle, etc.  
> &mdash;_(verb phrase)_ **muster in**, to enlist into service (in the armed forces).

Muster is a .NET (4.0) project aimed at simplifying most aspects of Windows Services &mdash; debugging, testing and deploying.

The core project, Muster, is a service abstraction layer and harnesses (console/service) based on a series of blog posts ([part one][p1], [part two][p2]) by [James Michael Hare][blog].

[p1]: http://geekswithblogs.net/BlackRabbitCoder/archive/2010/09/23/c-windows-services-1-of-2-creating-a-debuggable-windows.aspx 
[p2]:http://geekswithblogs.net/BlackRabbitCoder/archive/2010/10/07/c-windows-services-2-of-2-self-installing-windows-service-template.aspx 
[blog]: http://geekswithblogs.net/BlackRabbitCoder/Default.aspx 

The runner project, Muster.Runner or `musterin`, is a console application that can run or install services found in arbitrary assemblies.

More documentation will follow. This project is currently considered unstable and not suitable for production use. This project, including the bundled Mono.Options source, is licenced under the [MIT licence][mit].

[mit]: http://www.opensource.org/licenses/mit-license.html
