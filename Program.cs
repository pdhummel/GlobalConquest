using GlobalConquest;

var game = new GlobalConquestGame();
Globals.Log("Game starting");
//game.Run();
try
{
    game.Run();
}
catch(ObjectDisposedException eIgnore)
{
}
Globals.Log("Game exited");
