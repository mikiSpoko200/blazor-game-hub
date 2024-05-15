# GameHub

Hub for real time online games. Currently only game implemented is TicTacToe, but hub can support arbitrary games.

This project contains both client and server applications. Client runs blazor WASM and server ASP.NET core.
Clients can create game lobbies or join existing ones which server manages.

## Stack

Client and server communicate using REST and SignalR. REST API provides basic information about active lobbies, as well as access to account for authenticated users. 

SignalR connection is initialized once player joins a lobby. Then server connects users within lobby using SignalR Hub which allows for push notifications and real time updates.
