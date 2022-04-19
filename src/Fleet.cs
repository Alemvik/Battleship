namespace Battleship;

public class Fleet
{
	public readonly static string MinCoordinate = "A1";
	private static string _maxCoordinate = "J10";
	public static string MaxCoordinate {get => _maxCoordinate; set {
		Coordinate.Max = Coordinate.Parse(value,false);
		_maxCoordinate = Coordinate.Max.ToString();
	}}

	enum EDamage {Miss, Hit, Sank, Decimated};
	string Name {get; init;}
	bool IsHumanControlled {get; init;}
	readonly List<Ship> ships = new();
	List<Coordinate> HitLocations {get;} = new();
	int NbShipLeft => ships.Where(s=>!s.IsSunk()).Count();

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

	(EDamage Damage, string ShipName) CheckIsHit(int col, int row, Fleet otherFleet=null) {
		return CheckIsHit(new Coordinate(col, row), otherFleet);
	}

	(EDamage Damage, string ShipName) CheckIsHit(Coordinate shotCoordinate, Fleet otherFleet=null)
	{
		if (otherFleet == this) throw new ArgumentException("Other fleet must not be this one!");

		if (ships is null || shotCoordinate is null) return (EDamage.Miss,"");

		otherFleet?.HitLocations.Add(shotCoordinate);

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
			var coord = Coordinate.RandomCoordinate(HitLocations);
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
				if (HitLocations.Any(l => l.Col==c && l.Row==r)) str.Append(" *"); else str.Append(" ~");

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
				bool wasTargetted = otherFleet.HitLocations.Any(l => l.Col==c && l.Row==r);

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
			for (int c=1; c<=Coordinate.Max.Col ;c++) if (HitLocations.Any(l => l.Col==c && l.Row==r)) {
				var (dmg,shp) = otherFleet.CheckIsHit(c,r);
				Console.ForegroundColor = dmg==EDamage.Miss ? ConsoleColor.DarkBlue : ConsoleColor.DarkRed;
				if (dmg==EDamage.Sank || dmg==EDamage.Decimated) Console.Write($" {shp[0]}"); else Console.Write(" *");
			} else {
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write(" ~");
			}

			Console.Write('\n');
		}

		Console.ForegroundColor = fgColor;
	}

	class Ship
	{
		public string Name {get;}
		public int Length {get;}
		public List<Coordinate> Coordinates {get; set;} = new List<Coordinate>();

		public Ship(string name, int length, bool isHumanControlled, List<Coordinate> excludedCoordinates)
		{
			Name = name;
			Length = length;

			if (isHumanControlled) for(;;) {
				Console.Write($"Enter the left-top position of your {name} (its length is {length}). Letter first for vertical aligned: ");
				var input = Console.ReadLine();
				try {
					Coordinates = GetCoordinates(input,length,excludedCoordinates);
					break;
				} catch (Exception ex) {
					Console.WriteLine(ex.Message);
				}
			} else Coordinates = Coordinate.RandomRange(length,excludedCoordinates);
		}

		static List<Coordinate> GetCoordinates(string input, int length, IEnumerable<Coordinate> excludedCoordinates=null)
		{
			List<Coordinate> coordinates = new();
			var leftTopShipCoord = new Coordinate(input,excludedCoordinates);
			if (char.IsDigit(input[0])) {
				for (int i=0; i<length ;i++) coordinates.Add(new Coordinate(leftTopShipCoord.Col+i,leftTopShipCoord.Row,excludedCoordinates));
			} else for (int i=0; i<length ;i++) coordinates.Add(new Coordinate(leftTopShipCoord.Col,leftTopShipCoord.Row+i,excludedCoordinates));
			return coordinates;
		}

		public bool IsSunk()
		{
			return !Coordinates.Any(p => !p.IsHit);
		}

		public bool Occupies(int col, int row)
		{
			return Coordinates.Any(p => p.Col==col && p.Row==row);
		}

		public bool Occupies(Coordinate coordinate)
		{
			return Coordinates.Any(p => p==coordinate);
		}

		public override string ToString()
		{
			return Name;
		}
	} // class Ship

	class Coordinate {
		public static Coordinate Max {get; set;} = new Coordinate(10,10,true);
		public int Col {get; set;}
		public int Row {get; set;}
		public bool IsHit { get; set; } = false;

		public static Coordinate Parse(string input, bool validate=true) // example input: "A10", "10A"
		{
			//input = new Regex("[^a-zA-Z0-9]").Replace(input, "");
			int col, row;
			try {
				if (char.IsDigit(input[0])) {
					if (char.IsDigit(input[1])) {
						col = (int)(char.ToUpper(input[2])-'A'+1);
						row = int.Parse(input[0..2]);
					} else {
						col = (int)(char.ToUpper(input[1])-'A'+1);
						row = int.Parse(input[0..1]);
					}
				} else {
					col = (int)(char.ToUpper(input[0])-'A'+1);
					row = int.Parse(input[1..]);
				}
			} catch (Exception ex) {
				throw new ArgumentException($"\"{input}\" cannot be parsed. Try something like those valid coordinates: E5, E10, 5E, 10E. ", ex);
			}

			return validate ? new Coordinate(col,row) : new Coordinate(col,row,true);
		}

		Coordinate(int col, int row, bool noValidate) // fromn 5 x 5 to 26 x 26
		{
			Col = Math.Min(Math.Max(col,5),26);
			Row = Math.Min(Math.Max(row,5),26);
		}

		public Coordinate(string input, IEnumerable<Coordinate> excludedCoordinates=null) : this(Parse(input), excludedCoordinates) {}

		public Coordinate(char column, int row) : this(char.ToUpper(column) - 'A' + 1, row) {}

		public Coordinate(Coordinate coordinate, IEnumerable<Coordinate> excludedCoordinates=null) : this(coordinate.Col, coordinate.Row,excludedCoordinates) {}

		public Coordinate(int col, int row, IEnumerable<Coordinate> excludedCoordinates=null)
		{
			if (col>Max.Col || row>Max.Row) throw new ArgumentException($"Coodinate {(char)('A'+col-1)}{row} is invalide! Max is {(char)('A'+Max.Col-1)}{Max.Row}");

			Col = col;
			Row = row;

			if (excludedCoordinates is not null && excludedCoordinates.Any(s => s.Col==col && s.Row==row))
				throw new ArgumentException($"Coodinate {(char)('A'+col-1)}{row} is excluded.");
		}

		public static Coordinate RandomCoordinate(IEnumerable<Coordinate> excludedCoordinates=null)
		{
			var random = new Random();
			var coord = new Coordinate(random.Next(1,Max.Col+1), random.Next(1,Max.Row+1));
			if (excludedCoordinates is null) return coord;
			while (excludedCoordinates.Any(s => s == coord)) coord = new Coordinate(random.Next(1,Max.Col+1), random.Next(1,Max.Row+1));
			return coord;
		}

		public static List<Coordinate> RandomRange(int length, IEnumerable<Coordinate> excludedCoordinates=null) // contiguous and Orthogonal
		{
			var random = new Random();
			List<Coordinate> coordinates = new();
			int c,r;

	A:		for (;;) {
				bool IsHorizontally = random.Next(2)==1;

				if (IsHorizontally) { // left most column must be lower than Max.Col-length
					c = random.Next(1,Max.Col-length+2); // from 1 to Max.Col-length-1
					r = random.Next(1,Max.Row+1); // from 1 to Max.Row
					for (int i=0; i<length ;i++) if (excludedCoordinates.Any(e => e.Col==c+i && e.Row==r)) goto A;
					for (int i=0; i<length ;i++) coordinates.Add(new Coordinate(c+i,r));
				} else {
					c = random.Next(1,Max.Col+1); // from 1 to Max.Col
					r = random.Next(1,Max.Row-length+2); // from 1 to Max.Row-lenght-1
					for (int i=0; i<length ;i++) if (excludedCoordinates.Any(e => e.Col==c && e.Row==r+i)) goto A;
					for (int i=0; i<length ;i++) coordinates.Add(new Coordinate(c,r+i));
				}

				return coordinates;
			}
		}

		public override bool Equals(Object obj)
		{
			if (obj is null || !this.GetType().Equals(obj.GetType())) return false;
			var c = (Coordinate)obj;
			return (Col == c.Col) && (Row == c.Row);
		}

		public static bool operator == (Coordinate a, Coordinate b)
		{
			return a.Equals(b);
		}

		public static bool operator != (Coordinate a, Coordinate b)
		{
			return !a.Equals(b);
		}

		public override int GetHashCode()
		{
			return (Col << 8) | Row;
		}

		public override string ToString()
		{
			return $"{(char)('A'+Col-1)}{Row}";
		}
	} // class Coordinate
} // class Fleet

