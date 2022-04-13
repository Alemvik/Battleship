public class Coordinate {
	public static Coordinate Max {get; set;} = new Coordinate(10,10,true);
	public int Col {get; set;}
	public int Row {get; set;}
	public bool IsHit { get; set; } = false;

	public static Coordinate Parse(string input) // example input: "A10", "10A"
	{
		int col, row;
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

		return new Coordinate(col,row,true);
	}

	Coordinate(int col, int row, bool noValidation)
	{
		Col = col;
		Row = row;
	}

	public Coordinate(string input, IEnumerable<Coordinate> excludedCoordinates=null) : this(Parse(input), excludedCoordinates) {}

	public Coordinate(char column, int row) : this(char.ToUpper(column) - 'A' + 1, row) {}

	public Coordinate(Coordinate coordinate, IEnumerable<Coordinate> excludedCoordinates=null) : this(coordinate.Col, coordinate.Row,excludedCoordinates) {}

	public Coordinate(int col, int row, IEnumerable<Coordinate> excludedCoordinates=null)
	{
		if (col < 0 || col > 26) throw new ArgumentException($"Column {col} is invalid.");
		if (row < 0 || row > 26) throw new ArgumentException($"Row {row} is invalid.");

		if (col>Max.Col || row>Max.Row) throw new ArgumentException($"Coodinate {(char)('A'+col-1)}{row} is invalide! Max is {(char)('A'+Max.Col-1)}{Max.Row}");

		Col = col;
		Row = row;

		if (excludedCoordinates is not null && excludedCoordinates.Any(s => s.Col==col && s.Row==row))
			throw new ArgumentException($"Coodinate {(char)('A'+col-1)}{row} since it's occupied by another of your ship.");
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
}