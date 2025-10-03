using GlobalConquest;

//var game = new Game1(form.GetDrawSurface());
var game = new GlobalConquestGame();
Server server = new();
Client client = new();
game.Client = client;
game.Run();
Console.WriteLine("Program started");
