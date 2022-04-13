namespace Battleship;

public class Ship
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
		//coordinates.Add(leftTopShipCoord);
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
}