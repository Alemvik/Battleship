global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Text.RegularExpressions;

using Battleship;

Console.Title = "Battleship";
Console.BackgroundColor = ConsoleColor.Black;
Console.Clear();
Console.WriteLine(@"
                                     |__
                                     |\/
                                     ---
                                     / | [
                              !      | |||
                            _/|     _/|-++'
                        +  +--|    |--|--|_ |-
                      { /|__|  |/\__|  |--- |||__/
                     +---------------___[}-_===_.'____                 /\
                 ____`-' ||___-{]_| _[}-  |     |_[___\==--            \/   _
  __..._____--==/___]_|__|_____________________________[___\==--____,------' .7
 |                           Welcome to Battleship                      BB-61/
  \_________________________________________________________________________|");

if (args.Length>=1) try {
	Fleet.MaxCoordinate = args[0]; // eg. E5 for 5 x 5 grid
} catch (Exception ex) {
	Console.WriteLine(ex.Message + ex.InnerException?.Message);
	return;
}

Fleet fleetA;
Fleet fleetB;

PlaceShips();
Play();

void PlaceShips()
{
	fleetA = new Fleet(name:"You",isHumanControlled:true);
	fleetB = new Fleet(name:"Bot",isHumanControlled:false);
}

void Play()
{
	var fleets = new Fleet[] {fleetA,fleetB};
	int ix = new Random().Next(2);
	for (;;) {
		if (fleets[ix].Play(fleets[(ix+1)%2])) break;
		if (fleets[(ix+1)%2].Play(fleets[ix])) break;
		fleetA.PrintToConsole(fleetB);
	}
	fleetB.PrintToConsole(fleetA);
}