﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GedcomObfuscationLib
{
    public class NamesPools
    {
        public static string[] Surnames =
        {
            "ABBOTT",
            "ADAMS",
            "ADKINS",
            "ALLISON",
            "ANDERSEN",
            "ANDERSON",
            "ANDREWS",
            "ARCHER",
            "ARMSTRONG",
            "ARNOLD",
            "ASHLEY",
            "ATKINSON",
            "AVERY",
            "AYERS",
            "BAILEY",
            "BAKER",
            "BALDWIN",
            "BALL",
            "BALLARD",
            "BARBER",
            "BARKER",
            "BARNETT",
            "BARR",
            "BARRETT",
            "BARRY",
            "BARTLETT",
            "BARTON",
            "BATES",
            "BAUER",
            "BAXTER",
            "BEAN",
            "BEARD",
            "BECK",
            "BECKER",
            "BENDER",
            "BENNETT",
            "BENSON",
            "BENTLEY",
            "BERG",
            "BERGER",
            "BEST",
            "BISHOP",
            "BLACK",
            "BLACKBURN",
            "BLAIR",
            "BLAKE",
            "BLANCHARD",
            "BLANKENSHIP",
            "BLEVINS",
            "BOND",
            "BOOTH",
            "BOWEN",
            "BOWERS",
            "BOWMAN",
            "BOYER",
            "BOYLE",
            "BRADSHAW",
            "BRADY",
            "BRANDT",
            "BRENNAN",
            "BREWER",
            "BRIGGS",
            "BROCK",
            "BROWNING",
            "BRUCE",
            "BRYAN",
            "BUCHANAN",
            "BUCK",
            "BUCKLEY",
            "BURCH",
            "BURGESS",
            "BURKE",
            "BURNS",
            "BUSH",
            "CAIN",
            "CALLAHAN",
            "CAMERON",
            "CAMPBELL",
            "CANTRELL",
            "CAREY",
            "CARLSON",
            "CARPENTER",
            "CARR",
            "CARROLL",
            "CARSON",
            "CASE",
            "CASEY",
            "CHANDLER",
            "CHAPMAN",
            "CHASE",
            "CHRISTENSEN",
            "CHURCH",
            "CLARK",
            "CLEMENTS",
            "CLINE",
            "COCHRAN",
            "COFFEY",
            "COHEN",
            "COLE",
            "COLLINS",
            "COMBS",
            "COMPTON",
            "CONLEY",
            "CONNER",
            "CONRAD",
            "CONWAY",
            "COOK",
            "COSTA",
            "COX",
            "CRAIG",
            "CRANE",
            "CROSS",
            "CUMMINGS",
            "CUNNINGHAM",
            "CURTIS",
            "DALTON",
            "DAUGHERTY",
            "DAVENPORT",
            "DAVIDSON",
            "DAY",
            "DEAN",
            "DECKER",
            "DICKSON",
            "DILLON",
            "DODSON",
            "DONALDSON",
            "DONOVAN",
            "DOUGHERTY",
            "DOYLE",
            "DRAKE",
            "DUFFY",
            "DUKE",
            "DUNCAN",
            "DUNLAP",
            "DUNN",
            "DURHAM",
            "DYER",
            "EATON",
            "ELLIOTT",
            "ENGLISH",
            "ERICKSON",
            "ESTES",
            "FARLEY",
            "FARMER",
            "FARRELL",
            "FAULKNER",
            "FERGUSON",
            "FINLEY",
            "FISCHER",
            "FISHER",
            "FITZGERALD",
            "FITZPATRICK",
            "FLEMING",
            "FLETCHER",
            "FLYNN",
            "FOLEY",
            "FOWLER",
            "FOX",
            "FRANK",
            "FREDERICK",
            "FRENCH",
            "FRIEDMAN",
            "FROST",
            "FRY",
            "FRYE",
            "FULLER",
            "GALLAGHER",
            "GARDNER",
            "GARNER",
            "GARRISON",
            "GATES",
            "GENTRY",
            "GIBSON",
            "GILBERT",
            "GILLESPIE",
            "GLASS",
            "GOLDEN",
            "GOOD",
            "GOODMAN",
            "GOODWIN",
            "GOULD",
            "GREER",
            "GREGORY",
            "GRIFFITH",
            "GRIMES",
            "GROSS",
            "HAHN",
            "HAIL",
            "HALE",
            "HALEY",
            "HALL",
            "HAMMOND",
            "HANCOCK",
            "HANNA",
            "HANSEN",
            "HANSON",
            "HARDIN",
            "HARDING",
            "HARMON",
            "HARRINGTON",
            "HART",
            "HARTMAN",
            "HAYDEN",
            "HEATH",
            "HEBERT",
            "HENDRICKS",
            "HENDRIX",
            "HENSLEY",
            "HENSON",
            "HERMAN",
            "HERRING",
            "HESS",
            "HESTER",
            "HICKMAN",
            "HIGGINS",
            "HOBBS",
            "HODGES",
            "HOFFMAN",
            "HOGAN",
            "HOLLAND",
            "HOLT",
            "HOOD",
            "HOOVER",
            "HOPKINS",
            "HORN",
            "HOUSE",
            "HOWE",
            "HOWELL",
            "HUBBARD",
            "HUBER",
            "HUFF",
            "HUFFMAN",
            "HUGHES",
            "HULL",
            "HUMPHREY",
            "HUNT",
            "HURLEY",
            "HURST",
            "HUTCHINSON",
            "JACOBS",
            "JACOBSON",
            "JARVIS",
            "JENSEN",
            "JOHNS",
            "JOHNSTON",
            "KANE",
            "KEITH",
            "KELLER",
            "KELLEY",
            "KELLY",
            "KEMP",
            "KENNEDY",
            "KENT",
            "KERR",
            "KIRBY",
            "KIRK",
            "KLEIN",
            "KLINE",
            "KNAPP",
            "KNIGHT",
            "KOCH",
            "KRAMER",
            "KRUEGER",
            "LAMB",
            "LAMBERT",
            "LANDRY",
            "LANE",
            "LANG",
            "LARSEN",
            "LARSON",
            "LAWSON",
            "LEACH",
            "LEBLANC",
            "LEONARD",
            "LESTER",
            "LEVY",
            "LIVINGSTON",
            "LLOYD",
            "LONG",
            "LOWE",
            "LYNCH",
            "LYNN",
            "LYONS",
            "MACDONALD",
            "MADDEN",
            "MAHONEY",
            "MANN",
            "MANNING",
            "MARKS",
            "MARSH",
            "MARTIN",
            "MASSEY",
            "MATHEWS",
            "MAXWELL",
            "MAY",
            "MAYER",
            "MAYNARD",
            "MCBRIDE",
            "MCCANN",
            "MCCARTHY",
            "MCCARTY",
            "MCCLURE",
            "MCCONNELL",
            "MCCORMICK",
            "MCCULLOUGH",
            "MCDANIEL",
            "MCDONALD",
            "MCFARLAND",
            "MCGUIRE",
            "MCINTYRE",
            "MCKAY",
            "MCKEE",
            "MCLAUGHLIN",
            "MCMAHON",
            "MCPHERSON",
            "MEADOWS",
            "MELTON",
            "MERRITT",
            "MEYER",
            "MEYERS",
            "MICHAEL",
            "MILLER",
            "MILLS",
            "MOODY",
            "MORGAN",
            "MORRIS",
            "MORRISON",
            "MORROW",
            "MORSE",
            "MORTON",
            "MOYER",
            "MUELLER",
            "MULLEN",
            "MULLINS",
            "MURPHY",
            "MURRAY",
            "MYERS",
            "NELSON",
            "NEWMAN",
            "NEWTON",
            "NICHOLS",
            "NICHOLSON",
            "NIELSEN",
            "NOBLE",
            "NOLAN",
            "NORRIS",
            "NORTON",
            "NOVAK",
            "OBRIEN",
            "OCONNELL",
            "OCONNOR",
            "ODONNELL",
            "OLSEN",
            "OLSON",
            "ONEILL",
            "ORR",
            "OSBORNE",
            "OWEN",
            "PACE",
            "PAGE",
            "PALMER",
            "PARRISH",
            "PARSONS",
            "PATRICK",
            "PATTON",
            "PAYNE",
            "PEARSON",
            "PECK",
            "PENNINGTON",
            "PETERS",
            "PETERSEN",
            "PETERSON",
            "PHELPS",
            "PHILLIPS",
            "PIERCE",
            "POOLE",
            "POTTER",
            "POTTS",
            "POWERS",
            "PRATT",
            "PRESTON",
            "PRICE",
            "PRUITT",
            "QUINN",
            "RAMSEY",
            "RANDALL",
            "RASMUSSEN",
            "RAY",
            "RAYMOND",
            "REED",
            "REEVES",
            "REILLY",
            "REYNOLDS",
            "RHODES",
            "RICE",
            "RICH",
            "RICHARDS",
            "RICHMOND",
            "RILEY",
            "ROACH",
            "ROBBINS",
            "ROBERTS",
            "ROBERTSON",
            "ROGERS",
            "ROSE",
            "ROTH",
            "ROWE",
            "ROWLAND",
            "ROY",
            "RUSH",
            "RUSSELL",
            "RUSSO",
            "RYAN",
            "SANFORD",
            "SAVAGE",
            "SAWYER",
            "SCHAEFER",
            "SCHMIDT",
            "SCHMITT",
            "SCHNEIDER",
            "SCHROEDER",
            "SCHULTZ",
            "SCHWARTZ",
            "SELLERS",
            "SEXTON",
            "SHAFFER",
            "SHANNON",
            "SHARP",
            "SHAW",
            "SHELTON",
            "SHEPARD",
            "SHEPHERD",
            "SHERMAN",
            "SHIELDS",
            "SHORT",
            "SKINNER",
            "SLOAN",
            "SNOW",
            "SNYDER",
            "SPARKS",
            "SPENCE",
            "STAFFORD",
            "STANLEY",
            "STANTON",
            "STARK",
            "STEELE",
            "STEIN",
            "STEPHENS",
            "STEPHENSON",
            "STEVENS",
            "STONE",
            "STOUT",
            "STRICKLAND",
            "STUART",
            "SULLIVAN",
            "SUMMERS",
            "SUTTON",
            "SWANSON",
            "SWEENEY",
            "TANNER",
            "TODD",
            "TRAVIS",
            "UNDERWOOD",
            "VANCE",
            "VAUGHAN",
            "VAUGHN",
            "VINCENT",
            "WAGNER",
            "WALL",
            "WALSH",
            "WALTER",
            "WALTERS",
            "WARD",
            "WARNER",
            "WATERS",
            "WEAVER",
            "WEBB",
            "WEBER",
            "WEBSTER",
            "WEEKS",
            "WEISS",
            "WELCH",
            "WELLS",
            "WEST",
            "WHEELER",
            "WHITNEY",
            "WILCOX",
            "WILKINSON",
            "WILLIAMSON",
            "WINTERS",
            "WISE",
            "WOLF",
            "WOLFE",
            "WOOD",
            "WOODWARD",
            "WYATT",
            "YATES",
            "YODER",
            "YORK",
            "ZIMMERMAN",
        };

        public static string[] FemaleNames =
        {
        "ADA",
        "ADRIENNE",
        "AGNES",
        "ALBERTA",
        "ALEXANDRA",
        "ALEXIS",
        "ALICE",
        "ALICIA",
        "ALISON",
        "ALLISON",
        "ALMA",
        "ALYSSA",
        "AMANDA",
        "AMBER",
        "AMELIA",
        "AMY",
        "ANA",
        "ANDREA",
        "ANGEL",
        "ANGELA",
        "ANGELICA",
        "ANGELINA",
        "ANGIE",
        "ANITA",
        "ANN",
        "ANNA",
        "ANNE",
        "ANNETTE",
        "ANNIE",
        "ANTOINETTE",
        "ANTONIA",
        "APRIL",
        "ARLENE",
        "ASHLEY",
        "AUDREY",
        "BARBARA",
        "BEATRICE",
        "BECKY",
        "BELINDA",
        "BERNADETTE",
        "BERNICE",
        "BERTHA",
        "BESSIE",
        "BETH",
        "BETHANY",
        "BETSY",
        "BETTY",
        "BEULAH",
        "BEVERLY",
        "BILLIE",
        "BLANCA",
        "BLANCHE",
        "BOBBIE",
        "BONNIE",
        "BRANDI",
        "BRANDY",
        "BRENDA",
        "BRIDGET",
        "BRITTANY",
        "BROOKE",
        "CAMILLE",
        "CANDACE",
        "CANDICE",
        "CARLA",
        "CARMEN",
        "CAROL",
        "CAROLE",
        "CAROLINE",
        "CAROLYN",
        "CARRIE",
        "CASEY",
        "CASSANDRA",
        "CATHERINE",
        "CATHY",
        "CECELIA",
        "CECILIA",
        "CELIA",
        "CHARLENE",
        "CHARLOTTE",
        "CHELSEA",
        "CHERYL",
        "CHRISTIE",
        "CHRISTINA",
        "CHRISTINE",
        "CHRISTY",
        "CINDY",
        "CLAIRE",
        "CLARA",
        "CLAUDIA",
        "COLLEEN",
        "CONNIE",
        "CONSTANCE",
        "CORA",
        "COURTNEY",
        "CRISTINA",
        "CRYSTAL",
        "CYNTHIA",
        "DAISY",
        "DANA",
        "DANIELLE",
        "DARLA",
        "DARLENE",
        "DAWN",
        "DEANNA",
        "DEBBIE",
        "DEBORAH",
        "DEBRA",
        "DELIA",
        "DELLA",
        "DELORES",
        "DENISE",
        "DESIREE",
        "DIANA",
        "DIANE",
        "DIANNA",
        "DIANNE",
        "DIXIE",
        "DOLORES",
        "DONNA",
        "DORA",
        "DOREEN",
        "DORIS",
        "DOROTHY",
        "EBONY",
        "EDITH",
        "EDNA",
        "EILEEN",
        "ELAINE",
        "ELEANOR",
        "ELENA",
        "ELISA",
        "ELIZABETH",
        "ELLA",
        "ELLEN",
        "ELOISE",
        "ELSA",
        "ELSIE",
        "ELVIRA",
        "EMILY",
        "EMMA",
        "ERICA",
        "ERIKA",
        "ERIN",
        "ERMA",
        "ERNESTINE",
        "ESSIE",
        "ESTELLE",
        "ESTHER",
        "ETHEL",
        "EULA",
        "EUNICE",
        "EVA",
        "EVELYN",
        "FAITH",
        "FANNIE",
        "FAYE",
        "FELICIA",
        "FLORA",
        "FLORENCE",
        "FRANCES",
        "FRANCIS",
        "FREDA",
        "GAIL",
        "GAYLE",
        "GENEVA",
        "GENEVIEVE",
        "GEORGIA",
        "GERALDINE",
        "GERTRUDE",
        "GINA",
        "GINGER",
        "GLADYS",
        "GLENDA",
        "GLORIA",
        "GRACE",
        "GRETCHEN",
        "GUADALUPE",
        "GWEN",
        "GWENDOLYN",
        "HANNAH",
        "HARRIET",
        "HATTIE",
        "HAZEL",
        "HEATHER",
        "HEIDI",
        "HELEN",
        "HENRIETTA",
        "HILDA",
        "HOLLY",
        "HOPE",
        "IDA",
        "INEZ",
        "IRENE",
        "IRIS",
        "IRMA",
        "ISABEL",
        "JACKIE",
        "JACQUELINE",
        "JACQUELYN",
        "JAIME",
        "JAMIE",
        "JAN",
        "JANA",
        "JANE",
        "JANET",
        "JANICE",
        "JANIE",
        "JANIS",
        "JASMINE",
        "JEAN",
        "JEANETTE",
        "JEANNE",
        "JEANNETTE",
        "JEANNIE",
        "JENNA",
        "JENNIE",
        "JENNIFER",
        "JENNY",
        "JESSICA",
        "JESSIE",
        "JILL",
        "JO",
        "JOAN",
        "JOANN",
        "JOANNA",
        "JOANNE",
        "JODI",
        "JODY",
        "JOHANNA",
        "JOHNNIE",
        "JOSEFINA",
        "JOSEPHINE",
        "JOY",
        "JOYCE",
        "JUANA",
        "JUANITA",
        "JUDITH",
        "JUDY",
        "JULIA",
        "JULIE",
        "JUNE",
        "KARA",
        "KAREN",
        "KARI",
        "KARLA",
        "KATE",
        "KATHERINE",
        "KATHLEEN",
        "KATHRYN",
        "KATHY",
        "KATIE",
        "KATRINA",
        "KAY",
        "KAYLA",
        "KELLEY",
        "KELLI",
        "KELLIE",
        "KELLY",
        "KENDRA",
        "KERRY",
        "KIM",
        "KIMBERLY",
        "KRISTA",
        "KRISTEN",
        "KRISTI",
        "KRISTIE",
        "KRISTIN",
        "KRISTINA",
        "KRISTINE",
        "KRISTY",
        "KRYSTAL",
        "LANA",
        "LATOYA",
        "LAURA",
        "LAUREN",
        "LAURIE",
        "LAVERNE",
        "LEAH",
        "LEE",
        "LEIGH",
        "LELA",
        "LENA",
        "LEONA",
        "LESLIE",
        "LETICIA",
        "LILA",
        "LILLIAN",
        "LILLIE",
        "LINDA",
        "LINDSAY",
        "LINDSEY",
        "LISA",
        "LOIS",
        "LOLA",
        "LORA",
        "LORENA",
        "LORENE",
        "LORETTA",
        "LORI",
        "LORRAINE",
        "LOUISE",
        "LUCIA",
        "LUCILLE",
        "LUCY",
        "LULA",
        "LUZ",
        "LYDIA",
        "LYNDA",
        "LYNETTE",
        "LYNN",
        "LYNNE",
        "MABEL",
        "MABLE",
        "MADELINE",
        "MAE",
        "MAGGIE",
        "MAMIE",
        "MANDY",
        "MARCELLA",
        "MARCIA",
        "MARGARET",
        "MARGARITA",
        "MARGIE",
        "MARGUERITE",
        "MARIA",
        "MARIAN",
        "MARIANNE",
        "MARIE",
        "MARILYN",
        "MARION",
        "MARJORIE",
        "MARLENE",
        "MARSHA",
        "MARTA",
        "MARTHA",
        "MARY",
        "MARYANN",
        "MATTIE",
        "MAUREEN",
        "MAXINE",
        "MAY",
        "MEGAN",
        "MEGHAN",
        "MELANIE",
        "MELBA",
        "MELINDA",
        "MELISSA",
        "MELODY",
        "MERCEDES",
        "MEREDITH",
        "MICHELE",
        "MICHELLE",
        "MILDRED",
        "MINDY",
        "MINNIE",
        "MIRANDA",
        "MIRIAM",
        "MISTY",
        "MOLLY",
        "MONA",
        "MONICA",
        "MONIQUE",
        "MURIEL",
        "MYRA",
        "MYRTLE",
        "NADINE",
        "NANCY",
        "NAOMI",
        "NATALIE",
        "NATASHA",
        "NELLIE",
        "NETTIE",
        "NICHOLE",
        "NICOLE",
        "NINA",
        "NORA",
        "NORMA",
        "OLGA",
        "OLIVE",
        "OLIVIA",
        "OLLIE",
        "OPAL",
        "ORA",
        "PAM",
        "PAMELA",
        "PAT",
        "PATRICIA",
        "PATSY",
        "PATTI",
        "PATTY",
        "PAULA",
        "PAULETTE",
        "PAULINE",
        "PEARL",
        "PEGGY",
        "PENNY",
        "PHYLLIS",
        "PRISCILLA",
        "RACHAEL",
        "RACHEL",
        "RAMONA",
        "RAQUEL",
        "REBECCA",
        "REGINA",
        "RENEE",
        "RHONDA",
        "RITA",
        "ROBERTA",
        "ROBIN",
        "ROBYN",
        "ROCHELLE",
        "ROSA",
        "ROSALIE",
        "ROSE",
        "ROSEMARIE",
        "ROSEMARY",
        "ROSIE",
        "ROXANNE",
        "RUBY",
        "RUTH",
        "SABRINA",
        "SADIE",
        "SALLY",
        "SAMANTHA",
        "SANDRA",
        "SANDY",
        "SARA",
        "SARAH",
        "SHANNON",
        "SHARI",
        "SHARON",
        "SHAWNA",
        "SHEILA",
        "SHELIA",
        "SHELLEY",
        "SHELLY",
        "SHERI",
        "SHERRI",
        "SHERRY",
        "SHERYL",
        "SHIRLEY",
        "SILVIA",
        "SONIA",
        "SONJA",
        "SONYA",
        "SOPHIA",
        "SOPHIE",
        "STACEY",
        "STACY",
        "STELLA",
        "STEPHANIE",
        "SUE",
        "SUSAN",
        "SUSIE",
        "SUZANNE",
        "SYLVIA",
        "TABITHA",
        "TAMARA",
        "TAMI",
        "TAMMY",
        "TANYA",
        "TARA",
        "TASHA",
        "TERESA",
        "TERI",
        "TERRI",
        "TERRY",
        "THELMA",
        "THERESA",
        "TIFFANY",
        "TINA",
        "TONI",
        "TONYA",
        "TRACEY",
        "TRACI",
        "TRACY",
        "TRICIA",
        "VALERIE",
        "VANESSA",
        "VELMA",
        "VERA",
        "VERNA",
        "VERONICA",
        "VICKI",
        "VICKIE",
        "VICKY",
        "VICTORIA",
        "VIOLA",
        "VIOLET",
        "VIRGINIA",
        "VIVIAN",
        "WANDA",
        "WENDY",
        "WHITNEY",
        "WILLIE",
        "WILMA",
        "WINIFRED",
        "YOLANDA",
        "YVETTE",
        "YVONNE",
        };

        public static string[] MaleNames =
        {
            "AARON",
            "ABEL",
            "ABRAHAM",
            "ADAM",
            "ADRIAN",
            "AL",
            "ALAN",
            "ALBERT",
            "ALBERTO",
            "ALEJANDRO",
            "ALEX",
            "ALEXANDER",
            "ALFONSO",
            "ALFRED",
            "ALFREDO",
            "ALLAN",
            "ALLEN",
            "ALONZO",
            "ALTON",
            "ALVIN",
            "AMOS",
            "ANDRE",
            "ANDRES",
            "ANDREW",
            "ANDY",
            "ANGEL",
            "ANGELO",
            "ANTHONY",
            "ANTONIO",
            "ARCHIE",
            "ARMANDO",
            "ARNOLD",
            "ARTHUR",
            "ARTURO",
            "AUBREY",
            "AUSTIN",
            "BARRY",
            "BEN",
            "BENJAMIN",
            "BENNIE",
            "BENNY",
            "BERNARD",
            "BERT",
            "BILL",
            "BILLY",
            "BLAKE",
            "BOB",
            "BOBBY",
            "BOYD",
            "BRAD",
            "BRADFORD",
            "BRADLEY",
            "BRANDON",
            "BRENDAN",
            "BRENT",
            "BRETT",
            "BRIAN",
            "BRUCE",
            "BRYAN",
            "BRYANT",
            "BYRON",
            "CALEB",
            "CALVIN",
            "CAMERON",
            "CARL",
            "CARLOS",
            "CARLTON",
            "CARROLL",
            "CARY",
            "CASEY",
            "CECIL",
            "CEDRIC",
            "CESAR",
            "CHAD",
            "CHARLES",
            "CHARLIE",
            "CHESTER",
            "CHRIS",
            "CHRISTIAN",
            "CHRISTOPHER",
            "CLARENCE",
            "CLARK",
            "CLAUDE",
            "CLAY",
            "CLAYTON",
            "CLIFFORD",
            "CLIFTON",
            "CLINT",
            "CLINTON",
            "CLYDE",
            "CODY",
            "COLIN",
            "CONRAD",
            "COREY",
            "CORNELIUS",
            "CORY",
            "COURTNEY",
            "CRAIG",
            "CURTIS",
            "DALE",
            "DALLAS",
            "DAMON",
            "DAN",
            "DANA",
            "DANIEL",
            "DANNY",
            "DARIN",
            "DARNELL",
            "DARREL",
            "DARRELL",
            "DARREN",
            "DARRIN",
            "DARRYL",
            "DARYL",
            "DAVE",
            "DAVID",
            "DEAN",
            "DELBERT",
            "DENNIS",
            "DEREK",
            "DERRICK",
            "DEVIN",
            "DEWEY",
            "DEXTER",
            "DOMINGO",
            "DOMINIC",
            "DOMINICK",
            "DON",
            "DONALD",
            "DONNIE",
            "DOUG",
            "DOUGLAS",
            "DOYLE",
            "DREW",
            "DUANE",
            "DUSTIN",
            "DWAYNE",
            "DWIGHT",
            "EARL",
            "EARNEST",
            "ED",
            "EDDIE",
            "EDGAR",
            "EDMOND",
            "EDMUND",
            "EDUARDO",
            "EDWARD",
            "EDWIN",
            "ELBERT",
            "ELIAS",
            "ELIJAH",
            "ELLIS",
            "ELMER",
            "EMANUEL",
            "EMILIO",
            "EMMETT",
            "ENRIQUE",
            "ERIC",
            "ERICK",
            "ERIK",
            "ERNEST",
            "ERNESTO",
            "ERVIN",
            "EUGENE",
            "EVAN",
            "EVERETT",
            "FELIPE",
            "FELIX",
            "FERNANDO",
            "FLOYD",
            "FORREST",
            "FRANCIS",
            "FRANCISCO",
            "FRANK",
            "FRANKIE",
            "FRANKLIN",
            "FRED",
            "FREDDIE",
            "FREDERICK",
            "FREDRICK",
            "GABRIEL",
            "GARRETT",
            "GARRY",
            "GARY",
            "GENE",
            "GEOFFREY",
            "GEORGE",
            "GERALD",
            "GERARD",
            "GERARDO",
            "GILBERT",
            "GILBERTO",
            "GLEN",
            "GLENN",
            "GORDON",
            "GRADY",
            "GRANT",
            "GREG",
            "GREGG",
            "GREGORY",
            "GUADALUPE",
            "GUILLERMO",
            "GUSTAVO",
            "GUY",
            "HAROLD",
            "HARRY",
            "HARVEY",
            "HECTOR",
            "HENRY",
            "HERBERT",
            "HERMAN",
            "HOMER",
            "HORACE",
            "HOWARD",
            "HUBERT",
            "HUGH",
            "HUGO",
            "IAN",
            "IGNACIO",
            "IRA",
            "IRVIN",
            "IRVING",
            "ISAAC",
            "ISMAEL",
            "ISRAEL",
            "IVAN",
            "JACK",
            "JACKIE",
            "JACOB",
            "JAIME",
            "JAKE",
            "JAMES",
            "JAMIE",
            "JAN",
            "JARED",
            "JASON",
            "JAVIER",
            "JAY",
            "JEAN",
            "JEFF",
            "JEFFERY",
            "JEFFREY",
            "JERALD",
            "JEREMIAH",
            "JEREMY",
            "JERMAINE",
            "JEROME",
            "JERRY",
            "JESSE",
            "JESSIE",
            "JESUS",
            "JIM",
            "JIMMIE",
            "JIMMY",
            "JODY",
            "JOE",
            "JOEL",
            "JOEY",
            "JOHN",
            "JOHNATHAN",
            "JOHNNIE",
            "JOHNNY",
            "JON",
            "JONATHAN",
            "JONATHON",
            "JORDAN",
            "JORGE",
            "JOSE",
            "JOSEPH",
            "JOSH",
            "JOSHUA",
            "JUAN",
            "JULIAN",
            "JULIO",
            "JULIUS",
            "JUSTIN",
            "KARL",
            "KEITH",
            "KELLY",
            "KELVIN",
            "KEN",
            "KENNETH",
            "KENNY",
            "KENT",
            "KERRY",
            "KEVIN",
            "KIM",
            "KIRK",
            "KRISTOPHER",
            "KURT",
            "KYLE",
            "LAMAR",
            "LANCE",
            "LARRY",
            "LAURENCE",
            "LAWRENCE",
            "LEE",
            "LELAND",
            "LEO",
            "LEON",
            "LEONARD",
            "LEROY",
            "LESLIE",
            "LESTER",
            "LEVI",
            "LEWIS",
            "LIONEL",
            "LLOYD",
            "LONNIE",
            "LOREN",
            "LORENZO",
            "LOUIS",
            "LOWELL",
            "LUCAS",
            "LUIS",
            "LUKE",
            "LUTHER",
            "LYLE",
            "LYNN",
            "MACK",
            "MALCOLM",
            "MANUEL",
            "MARC",
            "MARCO",
            "MARCOS",
            "MARCUS",
            "MARIO",
            "MARION",
            "MARK",
            "MARLON",
            "MARSHALL",
            "MARTIN",
            "MARTY",
            "MARVIN",
            "MATHEW",
            "MATT",
            "MATTHEW",
            "MAURICE",
            "MAX",
            "MELVIN",
            "MERLE",
            "MICHAEL",
            "MICHEAL",
            "MIGUEL",
            "MIKE",
            "MILTON",
            "MITCHELL",
            "MORRIS",
            "MOSES",
            "MYRON",
            "NATHAN",
            "NATHANIEL",
            "NEAL",
            "NEIL",
            "NELSON",
            "NICHOLAS",
            "NICK",
            "NICOLAS",
            "NOAH",
            "NOEL",
            "NORMAN",
            "OLIVER",
            "OMAR",
            "ORLANDO",
            "ORVILLE",
            "OSCAR",
            "OTIS",
            "OWEN",
            "PABLO",
            "PAT",
            "PATRICK",
            "PAUL",
            "PEDRO",
            "PERCY",
            "PERRY",
            "PETE",
            "PETER",
            "PHIL",
            "PHILIP",
            "PHILLIP",
            "PRESTON",
            "RAFAEL",
            "RALPH",
            "RAMIRO",
            "RAMON",
            "RANDAL",
            "RANDALL",
            "RANDOLPH",
            "RANDY",
            "RAUL",
            "RAY",
            "RAYMOND",
            "REGINALD",
            "RENE",
            "REX",
            "RICARDO",
            "RICHARD",
            "RICK",
            "RICKEY",
            "RICKY",
            "ROBERT",
            "ROBERTO",
            "ROBIN",
            "RODERICK",
            "RODNEY",
            "RODOLFO",
            "ROGELIO",
            "ROGER",
            "ROLAND",
            "ROLANDO",
            "ROMAN",
            "RON",
            "RONALD",
            "RONNIE",
            "ROOSEVELT",
            "ROSS",
            "ROY",
            "RUBEN",
            "RUDOLPH",
            "RUDY",
            "RUFUS",
            "RUSSELL",
            "RYAN",
            "SALVADOR",
            "SALVATORE",
            "SAM",
            "SAMMY",
            "SAMUEL",
            "SANTIAGO",
            "SANTOS",
            "SAUL",
            "SCOTT",
            "SEAN",
            "SERGIO",
            "SETH",
            "SHANE",
            "SHANNON",
            "SHAUN",
            "SHAWN",
            "SHELDON",
            "SHERMAN",
            "SIDNEY",
            "SIMON",
            "SPENCER",
            "STANLEY",
            "STEPHEN",
            "STEVE",
            "STEVEN",
            "STEWART",
            "STUART",
            "SYLVESTER",
            "TAYLOR",
            "TED",
            "TERENCE",
            "TERRANCE",
            "TERRELL",
            "TERRENCE",
            "TERRY",
            "THEODORE",
            "THOMAS",
            "TIM",
            "TIMMY",
            "TIMOTHY",
            "TOBY",
            "TODD",
            "TOM",
            "TOMAS",
            "TOMMIE",
            "TOMMY",
            "TONY",
            "TRACY",
            "TRAVIS",
            "TREVOR",
            "TROY",
            "TYLER",
            "TYRONE",
            "VAN",
            "VERNON",
            "VICTOR",
            "VINCENT",
            "VIRGIL",
            "WADE",
            "WALLACE",
            "WALTER",
            "WARREN",
            "WAYNE",
            "WENDELL",
            "WESLEY",
            "WILBERT",
            "WILBUR",
            "WILFRED",
            "WILLARD",
            "WILLIAM",
            "WILLIE",
            "WILLIS",
            "WILSON",
            "WINSTON",
            "WM",
            "WOODROW",
            "ZACHARY",
        };

        private List<string> _availableSurnames;
        private List<string> _availableFnames;
        private List<string> _availableMnames;

        private Dictionary<string, string> _assignedSurnames;
        private Dictionary<string, string> _assignedFnames;
        private Dictionary<string, string> _assignedMnames;

        private Random _rand;

        public NamesPools()
        {
            _availableSurnames=new List<string>(Surnames);
            _availableFnames = new List<string>(FemaleNames);
            _availableMnames = new List<string>(MaleNames);
            _assignedSurnames = new Dictionary<string, string>();
            _assignedFnames = new Dictionary<string, string>();
            _assignedMnames = new Dictionary<string, string>();
            _rand = new Random(DateTime.Now.Millisecond);
        }

        public void AssignSurname(string s)
        {
            if (_availableSurnames.Count < 10)
                _availableSurnames = new List<string>(Surnames);
            Assign(s, _assignedSurnames, _availableSurnames);
        }
        public void AssignFname(string s)
        {
            if (_availableFnames.Count < 10)
                _availableFnames = new List<string>(FemaleNames);
            Assign(s, _assignedFnames, _availableFnames);
        }
        public void AssignMame(string s)
        {
            if (_availableMnames.Count < 10)
                _availableMnames = new List<string>(MaleNames);
            Assign(s, _assignedMnames, _availableMnames);
        }

        public string MappedSurname(string n) => _assignedSurnames.TryGetValue(n, out string rv) ? rv : n;
        public string MappedMname(string n) => _assignedMnames.TryGetValue(n, out string rv) ? rv : n;
        public string MappedFname(string n) => _assignedFnames.TryGetValue(n, out string rv) ? rv : n;


        private void Assign(string s, Dictionary<string, string> known, List<string> available)
        {
            if (s == null || s.Length < 3) return;
            s = Regex.Replace(s, "[^ .a-zA-Z]", "");
            if (s.Length < 3) return;
            //s = s.ToUpper();
            if (known.ContainsKey(s)) return;
            if (available.Count < 50)
                ;
            int choice = _rand.Next(0, available.Count - 1);
            known.Add(s, available[choice]);
            available.RemoveAt(choice);
        }

        public List<TextReplacer> GetReplacers()
        {
            List<TextReplacer> rvl = new List<TextReplacer>();
            BuildReplacers(rvl, _assignedSurnames);
            BuildReplacers(rvl, _assignedFnames);
            BuildReplacers(rvl, _assignedMnames);
            return rvl;
        }

        private void BuildReplacers(List<TextReplacer> list, Dictionary<string, string> map)
        {
            foreach (string key in map.Keys)
            {
                list.Add(new TextReplacer(){Input = key, Output = map[key]});
            }
        }
    }
}