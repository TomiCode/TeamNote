# TeamNote (v2)
A secure way to communicate with Your team on the local network.

## Requirements
* .NET Framework 4.5.2
* Visual Studio 2015 Community

### NuGet Packages
* BouncyCastle.OpenPGP 1.8.1
* Google.Protobuf 3.1.0

## Configuration
### Client
```javascript
{ 
    "clientLocale":"",
    "udpBroadcastPort":1337
}
```

<table>
 <tr>
  <td><code>clientLocale</code></td>
  <td>When the locale detection gets weird. Currently supporting only <code>base</code> and <code>pol</code>.</td>
 </tr>
 <tr>
  <td><code>udpBroadcastPort</code></td>
  <td>Discovery Server port configuration. Default <code>1337</code>.</td>
 </tr>
</table>

### Server
```javascript
{ 
    "configServiceBindAddress":"0.0.0.0",
    "configServicePort":1337,
    "listenAddress":"127.0.0.1",
    "listenPort":1330,
    "serverName":null
}
```

<table>
 <tr>
  <td><code>configServiceBindAddress</code></td>
  <td>Discovery service listening address.</td>
 </tr>
 <tr>
  <td><code>configServicePort</code></td>
  <td>Discovery service listening port.</td>
 </tr>
 <tr>
  <td><code>listenAddress</code></td>
  <td>Communication server address. This needs to be a valid ethernet address.</td>
 </tr>
 <tr>
  <td><code>listenPort</code></td>
  <td>Communication server port.</td>
 </tr>
 <tr>
  <td><code>serverName</code></td>
  <td>Server name. Visible to all connected clients.</td>
 </tr>
</table>

## Copyright
Copyright (c) 2017, Tomasz Król `tomicode at gmail dot com`