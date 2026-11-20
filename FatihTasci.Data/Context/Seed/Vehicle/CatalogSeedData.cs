// Marka ve model (Series) listesi SpareParts projesinin seed verisinden üretildi
// (SparePart.Infrastructure/Persistence/Seed/VehicleBrandSeed.cs ve VehicleModelSeed.cs); SpareParts'taki "model"
// seviyesi (örn. BMW 3 Serisi, VW Golf) bu projede Series'e karşılık gelir. Paketler (CarModel) motor + donanım
// seviyesinde örneklerdir. Logolar: FatihTasci.Server/wwwroot/images/brands/{slug}.png. Kaynakları:
// github.com/filippofilip95/car-logos-dataset, Wikimedia Commons / Wikipedia ve markaların resmi siteleri. Logosu
// bulunamayan markalarda (I-GO, Joyce, Luqi) geçici gri araba ikonu (Material Symbols, Apache 2.0) durur.

namespace FatihTasci.Data.Context.Seed.Vehicle;

internal static class CatalogSeedData
{
    /// <summary>
    /// (Id, ad, slug = logo dosyasının adı, [modeller: (Id, ad, [paketler: (Id, ad)])]). Marka sırası listedeki sıradır,
    /// modeller alfabetiktir (sayılar doğal sırada); SortOrder buradan gelir, model ve paket slug'ı addan üretilir.
    /// Yeni kayda o tabloda kullanılmamış bir Id verilir; seed Id'leri 100000'in altında kalır (panelden eklenenler
    /// 100000'den başlar).
    /// </summary>
    public static readonly (int Id, string Name, string Slug, (int Id, string Name, (int Id, string Name)[] Models)[] Series)[] Brands =
    [
        (1, "Abarth", "abarth", []),
        (2, "Acura", "acura", []),
        (3, "Aion", "aion", []),
        (4, "Alfa Romeo", "alfa-romeo",
        [
            (1, "145", []), (2, "146", []), (3, "147", []), (4, "155", []), (5, "156", []), (6, "159", []), (7, "166", []),
            (8, "Brera", []), (9, "Giulia", []), (10, "Giulietta", []), (11, "GT", []), (12, "Junior", []), (13, "MiTo", []),
            (14, "Spider", []), (15, "Stelvio", []), (16, "Tonale", [])
        ]),
        (5, "Alpine", "alpine", []),
        (6, "Anadol", "anadol", []),
        (7, "Arora", "arora", []),
        (8, "Aston Martin", "aston-martin", []),
        (9, "Audi", "audi",
        [
            (17, "80", []), (18, "100", []), (19, "A1", []),
            (20, "A3", [(1, "A3 Sportback 35 TFSI"), (2, "A3 Sedan 35 TFSI")]),
            (21, "A4", [(3, "A4 40 TDI quattro"), (4, "A4 Sedan 40 TFSI")]),
            (22, "A5", [(5, "A5 Sportback 40 TFSI")]),
            (23, "A6", [(6, "A6 40 TDI quattro"), (7, "A6 45 TFSI quattro")]),
            (24, "A7", [(8, "A7 Sportback 50 TDI quattro")]),
            (25, "A8", [(9, "A8 L 50 TDI quattro")]),
            (26, "e-tron", []),
            (27, "e-tron GT", [(10, "e-tron GT quattro"), (11, "RS e-tron GT")]),
            (28, "Q2", []),
            (29, "Q3", [(12, "Q3 35 TFSI"), (13, "Q3 Sportback 35 TFSI")]),
            (30, "Q4 e-tron", []),
            (31, "Q5", [(14, "Q5 40 TDI quattro"), (129, "Q5 40 TDI quattro S line")]),
            (32, "Q7", [(15, "Q7 50 TDI quattro")]),
            (33, "Q8", [(16, "Q8 50 TDI quattro")]),
            (34, "Q8 e-tron", []), (35, "R8", []),
            (36, "RS", [(17, "RS 3"), (18, "RS 6 Avant"), (19, "RS Q8")]),
            (37, "TT", [])
        ]),
        (10, "Bajaj", "bajaj", []),
        (11, "Bentley", "bentley",
        [
            (38, "Bentayga", [(20, "Bentayga V8")]),
            (39, "Continental GT", [(21, "Continental GT V8")])
        ]),
        (12, "BMW", "bmw",
        [
            (40, "1 Serisi", [(22, "116i"), (23, "118i M Sport")]),
            (41, "2 Serisi", [(24, "218i Gran Coupe"), (25, "220i Coupe M Sport")]),
            (42, "3 Serisi", [(26, "320i"), (27, "320i M Sport"), (28, "320d xDrive"), (29, "M340i xDrive")]),
            (43, "4 Serisi", [(30, "420i Gran Coupe M Sport"), (31, "430i Cabrio")]),
            (44, "5 Serisi", [(32, "520i"), (33, "520d xDrive M Sport"), (34, "530e")]),
            (45, "6 Serisi", []),
            (46, "7 Serisi", [(35, "740d xDrive"), (36, "i7 xDrive60")]),
            (47, "8 Serisi", []),
            (48, "i Serisi", [(37, "i4 eDrive40"), (38, "iX xDrive50")]),
            (49, "i3", []), (50, "i4", []), (51, "i5", []), (52, "i7", []), (53, "iX", []), (54, "iX1", []), (55, "iX3", []),
            (56, "M Serisi", [(39, "M2"), (40, "M3 Competition"), (41, "M4 Competition"), (42, "M5")]),
            (57, "X1", [(43, "sDrive18i"), (44, "xDrive23d")]),
            (58, "X2", []),
            (59, "X3", [(45, "xDrive20d"), (46, "M40i")]),
            (60, "X4", []),
            (61, "X5", [(47, "xDrive40i M Sport"), (48, "xDrive50e")]),
            (62, "X6", [(49, "xDrive40i M Sport")]),
            (63, "X7", [(50, "xDrive40d")]),
            (64, "Z3", []), (65, "Z4", [])
        ]),
        (13, "Buick", "buick", []),
        (14, "BYD", "byd",
        [
            (66, "Atto 2", []), (67, "Atto 3", []), (68, "Dolphin", []), (69, "Han", []), (70, "Seal", []),
            (71, "Seal U", []), (72, "Sealion 7", []), (73, "Tang", [])
        ]),
        (15, "Cadillac", "cadillac", []),
        (16, "Chery", "chery",
        [
            (74, "Alia", []), (75, "Chance", []), (76, "Kimo", []), (77, "Niche", []), (78, "Tiggo", []), (79, "Tiggo 4", []),
            (80, "Tiggo 7", []), (81, "Tiggo 8", [])
        ]),
        (17, "Chevrolet", "chevrolet",
        [
            (82, "Aveo", []), (83, "Camaro", []), (84, "Captiva", []), (85, "Corvette", []), (86, "Cruze", []),
            (87, "Epica", []), (88, "Evanda", []), (89, "Kalos", []), (90, "Lacetti", []), (91, "Malibu", []),
            (92, "Orlando", []), (93, "Rezzo", []), (94, "Spark", []), (95, "Trax", [])
        ]),
        (18, "Chrysler", "chrysler", []),
        (19, "Citroen", "citroen",
        [
            (96, "Ami", []), (97, "AX", []), (98, "Berlingo", []), (99, "BX", []), (100, "C-Elysée", []), (101, "C1", []),
            (102, "C2", []), (103, "C3", []), (104, "C3 Aircross", []), (105, "C3 Picasso", []), (106, "C4", []),
            (107, "C4 Picasso", []), (108, "C4 X", []), (109, "C5", []), (110, "C5 Aircross", []), (111, "C5 X", []),
            (112, "C8", []), (113, "Jumper", []), (114, "Jumpy", []), (115, "Nemo", []), (116, "Saxo", []),
            (117, "Xantia", []), (118, "Xsara", []), (119, "Xsara Picasso", []), (120, "ZX", [])
        ]),
        (20, "Cupra", "cupra",
        [
            (121, "Ateca", []), (122, "Born", []), (123, "Formentor", []), (124, "Leon", []), (125, "Tavascan", []),
            (126, "Terramar", [])
        ]),
        (21, "Dacia", "dacia",
        [
            (127, "Bigster", []), (128, "Dokker", []), (129, "Duster", []), (130, "Jogger", []), (131, "Lodgy", []),
            (132, "Logan", []), (133, "Sandero", []), (134, "Solenza", []), (135, "Spring", [])
        ]),
        (22, "Daewoo", "daewoo", []),
        (23, "Daihatsu", "daihatsu", []),
        (24, "Dodge", "dodge", []),
        (25, "DS Automobiles", "ds-automobiles",
        [
            (136, "DS 3", []), (137, "DS 4", []), (138, "DS 5", []), (139, "DS 7", []), (140, "DS 9", [])
        ]),
        (26, "Eagle", "eagle", []),
        (27, "Ferrari", "ferrari", []),
        (28, "Fiat", "fiat",
        [
            (141, "500", []), (142, "500L", []), (143, "500X", []), (144, "600", []), (145, "Albea", []), (146, "Brava", []),
            (147, "Bravo", []), (148, "Doblo", []), (149, "Ducato", []), (150, "Egea", []), (151, "Egea Cross", []),
            (152, "Fiorino", []), (153, "Fullback", []), (154, "Grande Punto", []), (155, "Idea", []), (156, "Linea", []),
            (157, "Marea", []), (158, "Palio", []), (159, "Panda", []), (160, "Punto", []), (161, "Punto Evo", []),
            (162, "Scudo", []), (163, "Sedici", []), (164, "Siena", []), (165, "Stilo", []), (166, "Tempra", []),
            (167, "Tipo", []), (168, "Uno", [])
        ]),
        (29, "Ford", "ford",
        [
            (169, "B-Max", []), (170, "C-Max", []), (171, "EcoSport", []), (172, "Edge", []), (173, "Escort", []),
            (174, "Explorer", []), (175, "Fiesta", []), (176, "Focus", []), (177, "Fusion", []), (178, "Galaxy", []),
            (179, "Ka", []), (180, "Kuga", []), (181, "Mondeo", []), (182, "Mustang", []), (183, "Mustang Mach-E", []),
            (184, "Puma", [(130, "Puma 1.0 EcoBoost Style")]), (185, "Ranger", []), (186, "S-Max", []), (187, "Scorpio", []), (188, "Sierra", []),
            (189, "Taunus", []), (190, "Tourneo Connect", []), (191, "Tourneo Courier", []), (192, "Tourneo Custom", []),
            (193, "Transit", []), (194, "Transit Connect", []), (195, "Transit Courier", []), (196, "Transit Custom", [])
        ]),
        (30, "Geely", "geely", []),
        (31, "Honda", "honda",
        [
            (197, "Accord", []), (198, "City", []), (199, "Civic", []), (200, "CR-V", []), (201, "CR-Z", []),
            (202, "e:Ny1", []), (203, "HR-V", []), (204, "Insight", []), (205, "Jazz", []), (206, "Legend", []),
            (207, "Prelude", []), (208, "S2000", []), (209, "ZR-V", [])
        ]),
        (32, "Hyundai", "hyundai",
        [
            (210, "Accent", []), (211, "Accent Blue", []), (212, "Accent Era", []), (213, "Atos", []), (214, "Bayon", []),
            (215, "Coupe", []), (216, "Elantra", []), (217, "Excel", []), (218, "Getz", []), (219, "H-1", []),
            (220, "Ioniq", []), (221, "Ioniq 5", []), (222, "Ioniq 6", []), (223, "i10", []), (224, "i20", []),
            (225, "i30", []), (226, "i40", []), (227, "ix20", []), (228, "ix35", []), (229, "Kona", []), (230, "Matrix", []),
            (231, "Santa Fe", []), (232, "Sonata", []), (233, "Starex", []), (234, "Staria", []), (235, "Trajet", []),
            (236, "Tucson", [])
        ]),
        (33, "I-GO", "i-go", []),
        (34, "Ikco", "ikco", []),
        (35, "Infiniti", "infiniti", []),
        (36, "Isuzu", "isuzu", [(237, "D-Max", []), (238, "Trooper", [])]),
        (37, "Iveco", "iveco", []),
        (38, "Jaguar", "jaguar",
        [
            (239, "F-Pace", [(51, "F-Pace P250 R-Dynamic")]),
            (240, "F-Type", [(52, "F-Type P450 R-Dynamic")])
        ]),
        (39, "Jeep", "jeep",
        [
            (241, "Avenger", []), (242, "Cherokee", []), (243, "Commander", []), (244, "Compass", []),
            (245, "Grand Cherokee", []), (246, "Patriot", []), (247, "Renegade", []), (248, "Wrangler", [])
        ]),
        (40, "Joyce", "joyce", []),
        (41, "Kia", "kia",
        [
            (249, "Carens", []), (250, "Carnival", []), (251, "Ceed", []), (252, "Cerato", []), (253, "EV3", []),
            (254, "EV6", []), (255, "EV9", []), (256, "Magentis", []), (257, "Niro", []), (258, "Opirus", []),
            (259, "Picanto", []), (260, "Pride", []), (261, "ProCeed", []), (262, "Rio", []), (263, "Sorento", []),
            (264, "Soul", []), (265, "Sportage", []), (266, "Stinger", []), (267, "Stonic", []), (268, "Venga", []),
            (269, "XCeed", [])
        ]),
        (42, "Kuba", "kuba", []),
        (43, "Lada", "lada", []),
        (44, "Lamborghini", "lamborghini", []),
        (45, "Lancia", "lancia", []),
        (46, "Land Rover", "land-rover",
        [
            (270, "Defender", [(53, "Defender 110 D250"), (54, "Defender 90 P400")]),
            (271, "Discovery", [(55, "Discovery 3.0 D300")]),
            (272, "Discovery Sport", []), (273, "Freelander", []),
            (274, "Range Rover", [(56, "Range Rover 3.0 D350 Autobiography")]),
            (275, "Range Rover Evoque", [(57, "Evoque 1.5 P300e")]),
            (276, "Range Rover Sport", [(58, "Range Rover Sport 3.0 D300 Dynamic SE")]),
            (277, "Range Rover Velar", [(59, "Velar 2.0 P250 R-Dynamic")])
        ]),
        (47, "Leapmotor", "leapmotor", []),
        (48, "Lexus", "lexus",
        [
            (278, "ES", [(60, "ES 300h")]),
            (279, "NX", [(61, "NX 350h")]),
            (280, "RX", [(62, "RX 450h+")])
        ]),
        (49, "Lincoln", "lincoln", []),
        (50, "Lotus", "lotus", []),
        (51, "Luqi", "luqi", []),
        (52, "Marcos", "marcos", []),
        (53, "Maserati", "maserati",
        [
            (281, "Ghibli", [(63, "Ghibli GT")]),
            (282, "Grecale", [(64, "Grecale GT"), (65, "Grecale Trofeo")]),
            (283, "Levante", [(66, "Levante GT")])
        ]),
        (54, "Mazda", "mazda",
        [
            (284, "2", []), (285, "3", []), (286, "5", []), (287, "6", []), (288, "323", []), (289, "626", []),
            (290, "BT-50", []), (291, "CX-3", []), (292, "CX-5", []), (293, "CX-7", []), (294, "CX-9", []),
            (295, "CX-30", []), (296, "CX-60", []), (297, "MX-5", []), (298, "MX-30", []), (299, "Premacy", []),
            (300, "RX-8", [])
        ]),
        (55, "McLaren", "mclaren", []),
        (56, "Mercedes-Benz", "mercedes-benz",
        [
            (301, "A Serisi", [(67, "A 200 AMG"), (68, "A 180 d")]),
            (302, "AMG GT", [(69, "AMG GT 63 4MATIC+")]),
            (303, "B Serisi", []),
            (304, "C Serisi", [(70, "C 200 AMG"), (71, "C 220 d 4MATIC"), (72, "C 300 e")]),
            (305, "Citan", []),
            (306, "CLA", [(73, "CLA 200 AMG")]),
            (307, "CLE", [(74, "CLE 200 Coupe")]),
            (308, "CLK", []), (309, "CLS", []),
            (310, "E Serisi", [(75, "E 200 AMG"), (76, "E 220 d 4MATIC"), (77, "E 300 e"), (131, "E 200 d Exclusive")]),
            (311, "EQ", [(78, "EQE 350+"), (79, "EQS 450+")]),
            (312, "EQA", []), (313, "EQB", []), (314, "EQC", []), (315, "EQE", []), (316, "EQS", []),
            (317, "G Serisi", [(80, "G 400 d"), (81, "G 63 AMG")]),
            (318, "GLA", [(82, "GLA 200 AMG")]),
            (319, "GLB", []),
            (320, "GLC", [(83, "GLC 220 d 4MATIC"), (84, "GLC 300 e 4MATIC")]),
            (321, "GLE", [(85, "GLE 350 de 4MATIC"), (86, "GLE 450 4MATIC")]),
            (322, "GLK", []),
            (323, "GLS", [(87, "GLS 450 4MATIC")]),
            (324, "ML", []),
            (325, "S Serisi", [(88, "S 400 d 4MATIC"), (89, "S 580 4MATIC")]),
            (326, "SL", [(90, "SL 43 AMG"), (91, "SL 63 AMG 4MATIC+")]),
            (327, "SLK", []), (328, "Sprinter", []), (329, "V Serisi", []), (330, "Vaneo", []), (331, "Viano", []),
            (332, "Vito", [])
        ]),
        (57, "MG", "mg",
        [
            (333, "Cyberster", []), (334, "EHS", []), (335, "HS", []), (336, "Marvel R", []), (337, "MG3", []),
            (338, "MG4", []),
            (339, "MG7", [(92, "MG7 1.5 Passion"), (93, "MG7 1.5 Excellence"), (94, "MG7 1.5 Excellence Red Edition")]),
            (340, "ZS", [])
        ]),
        (58, "Micro", "micro", []),
        (59, "Mini", "mini",
        [
            (341, "Aceman", []), (342, "Cabrio", []), (343, "Clubman", []),
            (344, "Cooper", [(95, "Cooper S"), (96, "Cooper SE")]),
            (345, "Countryman", [(97, "Countryman S ALL4")]),
            (346, "Coupe", []), (347, "Paceman", [])
        ]),
        (60, "Mitsubishi", "mitsubishi",
        [
            (348, "ASX", []), (349, "Attrage", []), (350, "Carisma", []), (351, "Colt", []), (352, "Eclipse Cross", []),
            (353, "Galant", []), (354, "L200", []), (355, "Lancer", []), (356, "Outlander", []), (357, "Pajero", []),
            (358, "Space Star", []), (359, "Space Wagon", [])
        ]),
        (61, "Morgan", "morgan", []),
        (62, "Motolux", "motolux", []),
        (63, "Nieve", "nieve", []),
        (64, "Niğmer", "nigmer", []),
        (65, "Nissan", "nissan",
        [
            (360, "350Z", []), (361, "370Z", []), (362, "Almera", []), (363, "Ariya", []), (364, "Juke", []),
            (365, "Leaf", []), (366, "Micra", []), (367, "Murano", []), (368, "Navara", []), (369, "Note", []),
            (370, "NV200", []), (371, "Pathfinder", []), (372, "Patrol", []), (373, "Primera", []), (374, "Pulsar", []),
            (375, "Qashqai", []), (376, "Sunny", []), (377, "Terrano", []), (378, "Townstar", []), (379, "X-Trail", [])
        ]),
        (66, "Omoda", "omoda", []),
        (67, "Opel", "opel",
        [
            (380, "Adam", []), (381, "Agila", []), (382, "Antara", []), (383, "Ascona", []), (384, "Astra", []),
            (385, "Calibra", []), (386, "Combo", []), (387, "Corsa", []), (388, "Crossland", []), (389, "Frontera", []),
            (390, "Grandland", []), (391, "Insignia", []), (392, "Kadett", []), (393, "Karl", []), (394, "Meriva", []),
            (395, "Mokka", []), (396, "Movano", []), (397, "Omega", []), (398, "Signum", []), (399, "Tigra", []),
            (400, "Vectra", []), (401, "Vivaro", []), (402, "Zafira", [])
        ]),
        (68, "Ortimobil", "ortimobil", []),
        (69, "Peugeot", "peugeot",
        [
            (403, "106", []), (404, "107", []), (405, "108", []), (406, "205", []), (407, "206", []), (408, "207", []),
            (409, "208", []), (410, "301", []), (411, "306", []), (412, "307", []), (413, "308", []), (414, "405", []),
            (415, "406", []), (416, "407", []), (417, "408", []), (418, "508", []), (419, "607", []), (420, "2008", []),
            (421, "3008", []), (422, "5008", []), (423, "Bipper", []), (424, "Boxer", []), (425, "Expert", []),
            (426, "Partner", []), (427, "RCZ", []), (428, "Rifter", []), (429, "Traveller", [])
        ]),
        (70, "Plymouth", "plymouth", []),
        (71, "Polestar", "polestar", []),
        (72, "Pontiac", "pontiac", []),
        (73, "Porsche", "porsche",
        [
            (430, "718", []),
            (431, "911", [(98, "911 Carrera"), (99, "911 Carrera S"), (100, "911 Turbo S"), (101, "911 GT3")]),
            (432, "Boxster", []),
            (433, "Cayenne", [(102, "Cayenne"), (103, "Cayenne E-Hybrid"), (104, "Cayenne Coupe")]),
            (434, "Cayman", []),
            (435, "Macan", [(105, "Macan"), (106, "Macan 4 Electric")]),
            (436, "Panamera", [(107, "Panamera 4"), (108, "Panamera 4 E-Hybrid")]),
            (437, "Taycan", [(109, "Taycan"), (110, "Taycan 4S"), (111, "Taycan Turbo")])
        ]),
        (74, "Proton", "proton", []),
        (75, "Rainwoll", "rainwoll", []),
        (76, "Rebat Motor", "rebat-motor", []),
        (77, "Reeder", "reeder", []),
        (78, "Regal Raptor", "regal-raptor", []),
        (79, "Relive", "relive", []),
        (80, "Renault", "renault",
        [
            (438, "Arkana", []), (439, "Austral", []), (440, "Broadway", []), (441, "Captur", []), (442, "Clio", []),
            (443, "Espace", []), (444, "Express", []), (445, "Fluence", []), (446, "Kadjar", []), (447, "Kangoo", []),
            (448, "Koleos", []), (449, "Laguna", []), (450, "Latitude", []), (451, "Master", []), (452, "Megane", []),
            (453, "Megane E-Tech", []), (454, "Modus", []), (455, "R 9", []), (456, "R 11", []), (457, "R 12", []),
            (458, "R 19", []), (459, "R 21", []), (460, "Rafale", []), (461, "Safrane", []), (462, "Scenic", []),
            (463, "Symbol", []), (464, "Taliant", []), (465, "Toros", []), (466, "Trafic", []), (467, "Twingo", []),
            (468, "Twizy", []), (469, "Vel Satis", []), (470, "Zoe", [])
        ]),
        (81, "RKS", "rks", []),
        (82, "Rolls-Royce", "rolls-royce", []),
        (83, "Rover", "rover", []),
        (84, "Saab", "saab", []),
        (85, "Saipa", "saipa", []),
        (86, "Seat", "seat",
        [
            (471, "Alhambra", []), (472, "Altea", []), (473, "Arona", []), (474, "Ateca", []), (475, "Cordoba", []),
            (476, "Exeo", []), (477, "Ibiza", []), (478, "Leon", []), (479, "Mii", []), (480, "Tarraco", []),
            (481, "Toledo", [])
        ]),
        (87, "Skoda", "skoda",
        [
            (482, "Citigo", []), (483, "Elroq", []), (484, "Enyaq", []), (485, "Fabia", []), (486, "Favorit", []),
            (487, "Felicia", []), (488, "Kamiq", []), (489, "Karoq", []), (490, "Kodiaq", []), (491, "Octavia", []),
            (492, "Rapid", []), (493, "Roomster", []), (494, "Scala", []), (495, "Superb", []), (496, "Yeti", [])
        ]),
        (88, "Smart", "smart", []),
        (89, "SsangYong", "ssangyong", []),
        (90, "Subaru", "subaru",
        [
            (497, "BRZ", []), (498, "Crosstrek", []), (499, "Forester", []), (500, "Impreza", []), (501, "Legacy", []),
            (502, "Levorg", []), (503, "Outback", []), (504, "Solterra", []), (505, "XV", [])
        ]),
        (91, "Suzuki", "suzuki",
        [
            (506, "Alto", []), (507, "Baleno", []), (508, "Grand Vitara", []), (509, "Ignis", []), (510, "Jimny", []),
            (511, "Liana", []), (512, "S-Cross", []), (513, "Splash", []), (514, "Swift", []), (515, "SX4", []),
            (516, "Vitara", []), (517, "Wagon R", [])
        ]),
        (92, "Tata", "tata", []),
        (93, "Tesla", "tesla",
        [
            (518, "Model 3", [(112, "Model 3 Standard Range"), (113, "Model 3 Long Range AWD")]),
            (519, "Model S", [(114, "Model S Plaid")]),
            (520, "Model X", [(115, "Model X Plaid")]),
            (521, "Model Y", [(116, "Model Y RWD"), (117, "Model Y Long Range AWD")])
        ]),
        (94, "The London Taxi", "the-london-taxi", []),
        (95, "Tofaş", "tofas",
        [
            (522, "Doğan", []), (523, "Kartal", []), (524, "Murat 124", []), (525, "Murat 131", []), (526, "Serçe", []),
            (527, "Şahin", [])
        ]),
        (96, "TOGG", "togg", [(528, "T10F", []), (529, "T10X", [])]),
        (97, "Toyota", "toyota",
        [
            (530, "Auris", []), (531, "Avensis", []), (532, "Aygo", []), (533, "Aygo X", []), (534, "bZ4X", []),
            (535, "C-HR", []), (536, "Camry", []), (537, "Carina", []), (538, "Corolla", []), (539, "Corolla Cross", []),
            (540, "Corona", []), (541, "GR86", []), (542, "Hilux", []), (543, "Land Cruiser", []), (544, "Prius", []),
            (545, "Proace", []), (546, "Proace City", []), (547, "RAV4", []), (548, "Starlet", []), (549, "Supra", []),
            (550, "Urban Cruiser", []), (551, "Verso", []), (552, "Yaris", []), (553, "Yaris Cross", [])
        ]),
        (98, "Vanderhall", "vanderhall", []),
        (99, "Volkswagen", "volkswagen",
        [
            (554, "Amarok", []), (555, "Arteon", []), (556, "Beetle", []), (557, "Bora", []), (558, "Caddy", []),
            (559, "Caravelle", []), (560, "CC", []), (561, "Crafter", []),
            (562, "Golf", [(118, "Golf 1.5 eTSI R-Line"), (119, "Golf GTI")]),
            (563, "ID. Buzz", []), (564, "ID.3", []), (565, "ID.4", []), (566, "ID.5", []), (567, "ID.7", []),
            (568, "Jetta", []), (569, "Multivan", []),
            (570, "Passat", [(120, "Passat 1.5 eTSI Business"), (133, "Passat 1.6 TDI BlueMotion Impression")]),
            (571, "Phaeton", []), (572, "Polo", []), (573, "Scirocco", []), (574, "Sharan", []), (575, "T-Cross", []),
            (576, "T-Roc", []), (577, "Taigo", []),
            (578, "Tiguan", [(121, "Tiguan 1.5 eTSI R-Line")]),
            (579, "Touareg", [(122, "Touareg 3.0 V6 TDI R-Line")]),
            (580, "Touran", []), (581, "Transporter", []), (582, "Up!", []), (583, "Vento", [])
        ]),
        (100, "Volta", "volta", []),
        (101, "Volvo", "volvo",
        [
            (584, "850", []), (585, "C30", []), (586, "C40", []), (587, "EX30", []), (588, "EX90", []), (589, "S40", []),
            (590, "S60", [(123, "S60 B4 Ultimate")]),
            (591, "S80", []),
            (592, "S90", [(124, "S90 B5 Ultimate")]),
            (593, "V40", []), (594, "V50", []), (595, "V60", []), (596, "V70", []), (597, "V90", []),
            (598, "XC40", [(125, "XC40 B4 Plus")]),
            (599, "XC60", [(126, "XC60 B5 AWD Ultimate"), (127, "XC60 T8 Recharge"), (132, "XC60 B4 AWD Inscription")]),
            (600, "XC90", [(128, "XC90 B5 AWD Ultimate")])
        ]),
        (102, "XEV", "xev", []),
        (103, "Yuki", "yuki", []),
        (104, "Zlin Motors", "zlin-motors", [])
    ];
}
