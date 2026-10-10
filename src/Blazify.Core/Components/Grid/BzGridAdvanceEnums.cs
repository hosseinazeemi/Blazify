namespace BzCore.Enums;
public enum BzAdvCellType
{
    Text,
    Number,
    Currency,
    Percent,
    Date,
    Boolean,   // checkbox / tick
    Badge,     // colored chip
    Icon,      // font-awesome icon
    Image,     // <img>
    Link,      // <a>
    Html,      // raw html (trusted)
    Custom     // CellTemplate
}

public enum BzAdvPin { None, Left, Right }

public enum BzAdvFilterType { None, Text, Number, Date, Select, Boolean }

public enum BzAdvTextOp { Contains, Equals, StartsWith, EndsWith, NotContains }

public enum BzAdvNumOp { Eq, Ne, Gt, Gte, Lt, Lte, Between }

public enum BzAdvDensity { Comfortable, Cozy, Compact }

public enum BzAdvPaging { Pages, LoadMore, VirtualScroll, None }

public enum BzAdvSelection { None, Single, Multiple }

public enum BzAdvJoin { And, Or }
