public class Fleet
{
	public enum EDamage {Miss, Hit, Sank, Decimated};
	string Name {get; init;}
	bool IsHumanControlled {get; init;}
	List<Ship> ships = new List<Ship>();
	List<Coordinate> hitLocations {get;} = new();
	int NbShipLeft {get{return ships.Where(s=>!s.IsSunk()).Count();}}

	public Fleet(bool isHumanControlled, string name)
	{
		IsHumanControlled = isHumanControlled;
		Name = name;
		ships = new List<Ship>();

		ships.Add(new Ship(name:"Aircraft carrier", length:5, isHumanControlled:isHumanControlled, excludedCoordinates:ships.SelectMany(s => s.Coordinates).ToList()));
		ships.Add(new Ship(name:"Battleship", length:4, isHumanControlled:isHumanControlled, excludedCoordinates:ships.SelectMany(s => s.Coordinates).ToList()));
		ships.Add(new Ship(name:"Destroyer", length:3, isHumanControlled:isHumanControlled, excludedCoordinates:ships.SelectMany(s => s.Coordinates).ToList()));
		ships.Add(new Ship(name:"Patrol boat", length:2, isHumanControlled:isHumanControlled, excludedCoordinates:ships.SelectMany(s => s.Coordinates).ToList()));
		ships.Add(new Ship(name:"Submarine", length:3, isHumanControlled:isHumanControlled, excludedCoordinates:ships.SelectMany(s => s.Coordinates).ToList()));
	}

	public (EDamage Damage, string ShipName) CheckIsHit(int col, int row, Fleet otherFleet=null) {
		return CheckIsHit(new Coordinate(col, row), otherFleet);
	}

	public (EDamage Damage, string ShipName) CheckIsHit(Coordinate shotCoordinate, Fleet otherFleet=null)
	{
		if (otherFleet == this) throw new ArgumentException("Other fleet must not be this one!");

		if (ships is null || shotCoordinate is null) return (EDamage.Miss,"");

		otherFleet?.hitLocations.Add(shotCoordinate);

		//var ship = ships.FirstOrDefault(s => s.Coordinates.Any(c => c.Equals(shotCoordinate)));
		var ship = ships.FirstOrDefault(s => s.Occupies(shotCoordinate));
		if (ship is null) return (EDamage.Miss,"");

		var shipHitCoordinate = ship.Coordinates.First(c => c.Equals(shotCoordinate));
		if (otherFleet is not null) shipHitCoordinate.IsHit = true;

		if (ship.IsSunk()) {
			if (NbShipLeft==0) return (EDamage.Decimated,ship.Name);
			return (EDamage.Sank,ship.Name);
		}

		return (EDamage.Hit,ship.Name);
	}

	public bool Play(Fleet otherFleet)
	{
		int nbShipLeft = NbShipLeft;
		if (IsHumanControlled) for(;;) {
			Console.Write($"Enter the position of your {nbShipLeft} shot{(nbShipLeft > 0 ? "s" : "")} eg. A1, A4, D5. Game board size is from A to {(char)('A'+Coordinate.Max.Col-1)} and 1 to {Coordinate.Max.Row}) : ");
			var input = Console.ReadLine();
			try {
				var inputs = input.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
				if (inputs.Length > nbShipLeft) {
					Console.WriteLine($"Your number of shot must be from one up to the number of ship you have left (here it's {nbShipLeft})");
					continue;
				}
				var shootCoordinates = new List<Coordinate>();
				for (int i=0; i<inputs.Length ;i++) shootCoordinates.Add(new Coordinate(inputs[i]));
				for (int i=0; i<inputs.Length ;i++) {
					var (dam,shp) = otherFleet.CheckIsHit(shootCoordinates[i],this);
					switch(dam) {
						case EDamage.Decimated: Console.WriteLine($"{Name} ({shootCoordinates[i]}) just sank the last ship, the {shp}. {Name} won!"); return true;
						case EDamage.Sank: Console.WriteLine($"{Name} ({shootCoordinates[i]}) just sank the {shp}!"); break;
						case EDamage.Hit: Console.WriteLine($"{Name} ({shootCoordinates[i]}) just hit!"); break;
						default: Console.WriteLine($"{Name} ({shootCoordinates[i]}) missed!"); break;
					};
				}
				break;
			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
			}
		} else for (int i=0; i<nbShipLeft ;i++) {
			var coord = Coordinate.RandomCoordinate(hitLocations);
			var (dam,shp) = otherFleet.CheckIsHit(coord,this);
			switch(dam) {
				case EDamage.Decimated: Console.WriteLine($"{Name} ({coord}) just sank the last ship, the {shp}. {Name} won!"); return true;
				case EDamage.Sank: Console.WriteLine($"{Name} ({coord}) just sank the {shp}!"); break;
				case EDamage.Hit: Console.WriteLine($"{Name} ({coord}) just hit the {shp}!"); break;
				default: Console.WriteLine($"{Name} ({coord}) missed!"); break;
			};
		}

		return false;
	}

	public override string ToString()
	{
		var str = new StringBuilder("  ");

		for (int i=0; i<Coordinate.Max.Col ;i++) str.Append($" {(char)('A'+i)}");
		str.Append("     ");
		for (int i=0; i<Coordinate.Max.Col ;i++) str.Append($" {(char)('A'+i)}");
		str.Append('\n');

		for (int r=1; r<=Coordinate.Max.Row ;r++) {
			str.Append($"{r,2}");
			for (int c=1; c<=Coordinate.Max.Col ;c++)
				if (CheckIsHit(c,r).Damage == EDamage.Miss) str.Append(" ~"); else str.Append(" *");

			str.Append("   ");
			str.Append($"{r,2}");
			for (int c=1; c<=Coordinate.Max.Col ;c++)
				if (hitLocations.Any(l => l.Col==c && l.Row==r)) str.Append(" *"); else str.Append(" ~");

			str.Append('\n');
		}

		return str.ToString();
	}

	public void PrintToConsole(Fleet otherFleet)
	{
		var fgColor = Console.ForegroundColor;
		Console.Write("  ");

		for (int i=0; i<Coordinate.Max.Col ;i++) Console.Write($" {(char)('A'+i)}");
		Console.Write("     ");
		for (int i=0; i<Coordinate.Max.Col ;i++) Console.Write($" {(char)('A'+i)}");
		Console.Write('\n');

		for (int r=1; r<=Coordinate.Max.Row ;r++) {
			Console.ForegroundColor = fgColor;
			Console.Write($"{r,2}");
			for (int c=1; c<=Coordinate.Max.Col ;c++) {
				var (dmg,shp) = CheckIsHit(c,r);
				bool wasTargetted = otherFleet.hitLocations.Any(l => l.Col==c && l.Row==r);

				if (dmg == EDamage.Miss) {
					Console.ForegroundColor = ConsoleColor.Cyan;
					if (wasTargetted) Console.Write(" *");
					else Console.Write(" ~");
				} else {
					Console.ForegroundColor = wasTargetted ? ConsoleColor.DarkRed : ConsoleColor.DarkBlue;
					Console.Write($" {shp[0]}");
				}
			}

			Console.ForegroundColor = fgColor;
			Console.Write("   ");
			Console.Write($"{r,2}");
			for (int c=1; c<=Coordinate.Max.Col ;c++)
				if (hitLocations.Any(l => l.Col==c && l.Row==r)) {
					Console.ForegroundColor = otherFleet.CheckIsHit(c,r).Damage==EDamage.Miss ? ConsoleColor.DarkBlue : ConsoleColor.DarkRed;
					Console.Write(" *");
				} else {
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.Write(" ~");
				}

			Console.Write('\n');
		}

		Console.ForegroundColor = fgColor;
	}
}

