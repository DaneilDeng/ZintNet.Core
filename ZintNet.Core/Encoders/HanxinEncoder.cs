using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ZintNet.Core.Encoders
{
    internal class HanXinEncoder : SymbolEncoder
    {
        #region Tables
        // Data from table B1: Data capacity of Han Xin Code.
        private static int[] TotalCodewords = {
            25, 37, 50, 54, 69, 84, 100, 117, 136, 155, 161, 181, 203, 225, 249,
            273, 299, 325, 353, 381, 411, 422, 453, 485, 518, 552, 587, 623, 660,
            698, 737, 754, 794, 836, 878, 922, 966, 1011, 1058, 1105, 1126, 1175,
            1224, 1275, 1327, 1380, 1434, 1489, 1513, 1569, 1628, 1686, 1745, 1805,
            1867, 1929, 1992, 2021, 2086, 2151, 2218, 2286, 2355, 2425, 2496, 2528,
            2600, 2673, 2749, 2824, 2900, 2977, 3056, 3135, 3171, 3252, 3334, 3416,
            3500, 3585, 3671, 3758, 3798, 3886 };

        private static int[] DataCodewordsL1 = {
            21, 31, 42, 46, 57, 70, 84, 99, 114, 131, 135, 153, 171, 189, 209, 229,
            251, 273, 297, 321, 345, 354, 381, 407, 436, 464, 493, 523, 554, 586, 619,
            634, 666, 702, 738, 774, 812, 849, 888, 929, 946, 987, 1028, 1071, 1115,
            1160, 1204, 1251, 1271, 1317, 1368, 1416, 1465, 1517, 1569, 1621, 1674,
            1697, 1752, 1807, 1864, 1920, 1979, 2037, 2096, 2124, 2184, 2245, 2309,
            2372, 2436, 2501, 2568, 2633, 2663, 2732, 2800, 2870, 2940, 3011,
            3083, 3156, 3190, 3264 };

        private static int[] DataCodewordsL2 = {
            17, 25, 34, 38, 49, 58, 70, 81, 96, 109, 113, 127, 143, 157, 175, 191, 209,
            227, 247, 267, 287, 296, 317, 339, 362, 386, 411, 437, 462, 488, 515, 528,
            556, 586, 614, 646, 676, 707, 740, 773, 788, 823, 856, 893, 929, 966, 1004,
            1043, 1059, 1099, 1140, 1180, 1221, 1263, 1307, 1351, 1394, 1415, 1460,
            1505, 1552, 1600, 1649, 1697, 1748, 1770, 1820, 1871, 1925, 1976, 2030,
            2083, 2140, 2195, 2219, 2276, 2334, 2392, 2450, 2509, 2569, 2630, 2658,
            2720 };

        private static int[] DataCodewordsL3 = {
            13, 19, 26, 30, 37, 46, 54, 63, 74, 83, 87, 97, 109, 121, 135, 147, 161,
            175, 191, 205, 221, 228, 245, 261, 280, 298, 317, 337, 358, 376, 397, 408,
            428, 452, 474, 498, 522, 545, 572, 597, 608, 635, 660, 689, 717, 746, 774,
            805, 817, 847, 880, 910, 943, 975, 1009, 1041, 1076, 1091, 1126, 1161, 1198,
            1234, 1271, 1309, 1348, 1366, 1404, 1443, 1485, 1524, 1566, 1607, 1650, 1693,
            1713, 1756, 1800, 1844, 1890, 1935, 1983, 2030, 2050, 2098 };

        private static int[] DataCodewordsL4 = {
            9, 15, 20, 22, 27, 34, 40, 47, 54, 61, 65, 73, 81, 89, 99, 109, 119, 129,
            141, 153, 165, 168, 181, 195, 208, 220, 235, 251, 264, 280, 295, 302, 318,
            334, 352, 368, 386, 405, 424, 441, 450, 469, 490, 509, 531, 552, 574, 595, 605,
            627, 652, 674, 697, 721, 747, 771, 796, 809, 834, 861, 892, 914, 941, 969, 998,
            1012, 1040, 1069, 1099, 1130, 1160, 1191, 1222, 1253, 1269, 1300, 1334,
            1366, 1400, 1433, 1469, 1504, 1520, 1554 };

        // Value 'k' from Annex A.
        private static int[] ModuleK = {
            0, 0, 0, 14, 16, 16, 17, 18, 19, 20,
            14, 15, 16, 16, 17, 17, 18, 19, 20, 20,
            21, 16, 17, 17, 18, 18, 19, 19, 20, 20,
            21, 17, 17, 18, 18, 19, 19, 19, 20, 20,
            17, 17, 18, 18, 18, 19, 19, 19, 17, 17,
            18, 18, 18, 18, 19, 19, 19, 17, 17, 18,
            18, 18, 18, 19, 19, 17, 17, 17, 18, 18,
            18, 18, 19, 19, 17, 17, 17, 18, 18, 18,
            18, 18, 17, 17 };

        // Value 'r' from Annex A.
        private static int[] ModuleR = {
            0, 0, 0, 15, 15, 17, 18, 19, 20, 21,
            15, 15, 15, 17, 17, 19, 19, 19, 19, 21,
            21, 17, 16, 18, 17, 19, 18, 20, 19, 21,
            20, 17, 19, 17, 19, 17, 19, 21, 19, 21,
            18, 20, 17, 19, 21, 18, 20, 22, 17, 19,
            15, 17, 19, 21, 17, 19, 21, 18, 20, 15,
            17, 19, 21, 16, 18, 17, 19, 21, 15, 17,
            19, 21, 15, 17, 18, 20, 22, 15, 17, 19,
            21, 23, 17, 19 };

        // Value of 'm' from Annex A.
        private static int[] ModuleM = {
            0, 0, 0, 1, 1, 1, 1, 1, 1, 1,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 3, 3, 3, 3, 3, 3, 3, 3, 3,
            3, 4, 4, 4, 4, 4, 4, 4, 4, 4,
            5, 5, 5, 5, 5, 5, 5, 5, 6, 6,
            6, 6, 6, 6, 6, 6, 6, 7, 7, 7,
            7, 7, 7, 7, 7, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 9, 9, 9, 9, 9, 9,
            9, 9, 10, 10 };

        // Error correction block sizes from Table D1.
        private static int[] TableD1 = {
            // #blocks, k, 2t, #blocks, k, 2t, #blocks, k, 2t.
            1, 21, 4, 0, 0, 0, 0, 0, 0, // version 1
            1, 17, 8, 0, 0, 0, 0, 0, 0,
            1, 13, 12, 0, 0, 0, 0, 0, 0,
            1, 9, 16, 0, 0, 0, 0, 0, 0,
            1, 31, 6, 0, 0, 0, 0, 0, 0, // version 2
            1, 25, 12, 0, 0, 0, 0, 0, 0,
            1, 19, 18, 0, 0, 0, 0, 0, 0,
            1, 15, 22, 0, 0, 0, 0, 0, 0,
            1, 42, 8, 0, 0, 0, 0, 0, 0, // version 3
            1, 34, 16, 0, 0, 0, 0, 0, 0,
            1, 26, 24, 0, 0, 0, 0, 0, 0,
            1, 20, 30, 0, 0, 0, 0, 0, 0,
            1, 46, 8, 0, 0, 0, 0, 0, 0, // version 4
            1, 38, 16, 0, 0, 0, 0, 0, 0,
            1, 30, 24, 0, 0, 0, 0, 0, 0,
            1, 22, 32, 0, 0, 0, 0, 0, 0,
            1, 57, 12, 0, 0, 0, 0, 0, 0, // version 5
            1, 49, 20, 0, 0, 0, 0, 0, 0,
            1, 37, 32, 0, 0, 0, 0, 0, 0,
            1, 14, 20, 1, 13, 22, 0, 0, 0,
            1, 70, 14, 0, 0, 0, 0, 0, 0, // version 6
            1, 58, 26, 0, 0, 0, 0, 0, 0,
            1, 24, 20, 1, 22, 18, 0, 0, 0,
            1, 16, 24, 1, 18, 26, 0, 0, 0,
            1, 84, 16, 0, 0, 0, 0, 0, 0, // version 7
            1, 70, 30, 0, 0, 0, 0, 0, 0,
            1, 26, 22, 1, 28, 24, 0, 0, 0,
            2, 14, 20, 1, 12, 20, 0, 0, 0,
            1, 99, 18, 0, 0, 0, 0, 0, 0, // version 8
            1, 40, 18, 1, 41, 18, 0, 0, 0,
            1, 31, 26, 1, 32, 28, 0, 0, 0,
            2, 16, 24, 1, 15, 22, 0, 0, 0,
            1, 114, 22, 0, 0, 0, 0, 0, 0, // version 9
            2, 48, 20, 0, 0, 0, 0, 0, 0,
            2, 24, 20, 1, 26, 22, 0, 0, 0,
            2, 18, 28, 1, 18, 26, 0, 0, 0,
            1, 131, 24, 0, 0, 0, 0, 0, 0, // version 10
            1, 52, 22, 1, 57, 24, 0, 0, 0,
            2, 27, 24, 1, 29, 24, 0, 0, 0,
            2, 21, 32, 1, 19, 30, 0, 0, 0,
            1, 135, 26, 0, 0, 0, 0, 0, 0, // version 11
            1, 56, 24, 1, 57, 24, 0, 0, 0,
            2, 28, 24, 1, 31, 26, 0, 0, 0,
            2, 22, 32, 1, 21, 32, 0, 0, 0,
            1, 153, 28, 0, 0, 0, 0, 0, 0, // version 12
            1, 62, 26, 1, 65, 28, 0, 0, 0,
            2, 32, 28, 1, 33, 28, 0, 0, 0,
            3, 17, 26, 1, 22, 30, 0, 0, 0,
            1, 86, 16, 1, 85, 16, 0, 0, 0, // version 13
            1, 71, 30, 1, 72, 30, 0, 0, 0,
            2, 37, 32, 1, 35, 30, 0, 0, 0,
            3, 20, 30, 1, 21, 32, 0, 0, 0,
            1, 94, 18, 1, 95, 18, 0, 0, 0, // version 14
            2, 51, 22, 1, 55, 24, 0, 0, 0,
            3, 30, 26, 1, 31, 26, 0, 0, 0,
            4, 18, 28, 1, 17, 24, 0, 0, 0,
            1, 104, 20, 1, 105, 20, 0, 0, 0, // version 15
            2, 57, 24, 1, 61, 26, 0, 0, 0,
            3, 33, 28, 1, 36, 30, 0, 0, 0,
            4, 20, 30, 1, 19, 30, 0, 0, 0,
            1, 115, 22, 1, 114, 22, 0, 0, 0, // version 16
            2, 65, 28, 1, 61, 26, 0, 0, 0,
            3, 38, 32, 1, 33, 30, 0, 0, 0,
            5, 19, 28, 1, 14, 24, 0, 0, 0,
            1, 126, 24, 1, 125, 24, 0, 0, 0, // version 17
            2, 70, 30, 1, 69, 30, 0, 0, 0,
            4, 33, 28, 1, 29, 26, 0, 0, 0,
            5, 20, 30, 1, 19, 30, 0, 0, 0,
            1, 136, 26, 1, 137, 26, 0, 0, 0, //version 18
            3, 56, 24, 1, 59, 26, 0, 0, 0,
            5, 35, 30, 0, 0, 0, 0, 0, 0,
            6, 18, 28, 1, 21, 28, 0, 0, 0,
            1, 148, 28, 1, 149, 28, 0, 0, 0, // version 19
            3, 61, 26, 1, 64, 28, 0, 0, 0,
            7, 24, 20, 1, 23, 22, 0, 0, 0,
            6, 20, 30, 1, 21, 32, 0, 0, 0,
            3, 107, 20, 0, 0, 0, 0, 0, 0, // version 20
            3, 65, 28, 1, 72, 30, 0, 0, 0,
            7, 26, 22, 1, 23, 22, 0, 0, 0,
            7, 19, 28, 1, 20, 32, 0, 0, 0,
            3, 115, 22, 0, 0, 0, 0, 0, 0, // version 21
            4, 56, 24, 1, 63, 28, 0, 0, 0,
            7, 28, 24, 1, 25, 22, 0, 0, 0,
            8, 18, 28, 1, 21, 22, 0, 0, 0,
            2, 116, 22, 1, 122, 24, 0, 0, 0, // version 22
            4, 56, 24, 1, 72, 30, 0, 0, 0,
            7, 28, 24, 1, 32, 26, 0, 0, 0,
            8, 18, 28, 1, 24, 30, 0, 0, 0,
            3, 127, 24, 0, 0, 0, 0, 0, 0, // version 23
            5, 51, 22, 1, 62, 26, 0, 0, 0,
            7, 30, 26, 1, 35, 26, 0, 0, 0,
            8, 20, 30, 1, 21, 32, 0, 0, 0,
            2, 135, 26, 1, 137, 26, 0, 0, 0, // version 24
            5, 56, 24, 1, 59, 26, 0, 0, 0,
            7, 33, 28, 1, 30, 28, 0, 0, 0,
            11, 16, 24, 1, 19, 26, 0, 0, 0,
            3, 105, 20, 1, 121, 22, 0, 0, 0, // version 25
            5, 61, 26, 1, 57, 26, 0, 0, 0,
            9, 28, 24, 1, 28, 22, 0, 0, 0,
            10, 19, 28, 1, 18, 30, 0, 0, 0,
            2, 157, 30, 1, 150, 28, 0, 0, 0, // version 26
            5, 65, 28, 1, 61, 26, 0, 0, 0,
            8, 33, 28, 1, 34, 30, 0, 0, 0,
            10, 19, 28, 2, 15, 26, 0, 0, 0,
            3, 126, 24, 1, 115, 22, 0, 0, 0, // version 27
            7, 51, 22, 1, 54, 22, 0, 0, 0,
            8, 35, 30, 1, 37, 30, 0, 0, 0,
            15, 15, 22, 1, 10, 22, 0, 0, 0,
            4, 105, 20, 1, 103, 20, 0, 0, 0, // version 28
            7, 56, 24, 1, 45, 18, 0, 0, 0,
            10, 31, 26, 1, 27, 26, 0, 0, 0,
            10, 17, 26, 3, 20, 28, 1, 21, 28,
            3, 139, 26, 1, 137, 28, 0, 0, 0, // version 29
            6, 66, 28, 1, 66, 30, 0, 0, 0,
            9, 36, 30, 1, 34, 32, 0, 0, 0,
            13, 19, 28, 1, 17, 32, 0, 0, 0,
            6, 84, 16, 1, 82, 16, 0, 0, 0, // version 30
            6, 70, 30, 1, 68, 30, 0, 0, 0,
            7, 35, 30, 3, 33, 28, 1, 32, 28,
            13, 20, 30, 1, 20, 28, 0, 0, 0,
            5, 105, 20, 1, 94, 18, 0, 0, 0, // version 31
            6, 74, 32, 1, 71, 30, 0, 0, 0,
            11, 33, 28, 1, 34, 32, 0, 0, 0,
            13, 19, 28, 3, 16, 26, 0, 0, 0,
            4, 127, 24, 1, 126, 24, 0, 0, 0, // version 32
            7, 66, 28, 1, 66, 30, 0, 0, 0,
            12, 30, 24, 1, 24, 28, 1, 24, 30,
            15, 19, 28, 1, 17, 32, 0, 0, 0,
            7, 84, 16, 1, 78, 16, 0, 0, 0, // version 33
            7, 70, 30, 1, 66, 28, 0, 0, 0,
            12, 33, 28, 1, 32, 30, 0, 0, 0,
            14, 21, 32, 1, 24, 28, 0, 0, 0,
            5, 117, 22, 1, 117, 24, 0, 0, 0, // version 34
            8, 66, 28, 1, 58, 26, 0, 0, 0,
            11, 38, 32, 1, 34, 32, 0, 0, 0,
            15, 20, 30, 2, 17, 26, 0, 0, 0,
            4, 148, 28, 1, 146, 28, 0, 0, 0, // version 35
            8, 68, 30, 1, 70, 24, 0, 0, 0,
            10, 36, 32, 3, 38, 28, 0, 0, 0,
            16, 19, 28, 3, 16, 26, 0, 0, 0,
            4, 126, 24, 2, 135, 26, 0, 0, 0, // version 36
            8, 70, 28, 2, 43, 26, 0, 0, 0,
            13, 32, 28, 2, 41, 30, 0, 0, 0,
            17, 19, 28, 3, 15, 26, 0, 0, 0,
            5, 136, 26, 1, 132, 24, 0, 0, 0, // version 37
            5, 67, 30, 4, 68, 28, 1, 69, 28,
            14, 35, 30, 1, 32, 24, 0, 0, 0,
            18, 18, 26, 3, 16, 28, 1, 14, 28,
            3, 142, 26, 3, 141, 28, 0, 0, 0, // version 38
            8, 70, 30, 1, 73, 32, 1, 74, 32,
            12, 34, 30, 3, 34, 26, 1, 35, 28,
            18, 21, 32, 1, 27, 30, 0, 0, 0,
            5, 116, 22, 2, 103, 20, 1, 102, 20, // version 39
            9, 74, 32, 1, 74, 30, 0, 0, 0,
            14, 34, 28, 2, 32, 32, 1, 32, 30,
            19, 21, 32, 1, 25, 26, 0, 0, 0,
            7, 116, 22, 1, 117, 22, 0, 0, 0, // version 40
            11, 65, 28, 1, 58, 24, 0, 0, 0,
            15, 38, 32, 1, 27, 28, 0, 0, 0,
            20, 20, 30, 1, 20, 32, 1, 21, 32,
            6, 136, 26, 1, 130, 24, 0, 0, 0, // version 41
            11, 66, 28, 1, 62, 30, 0, 0, 0,
            14, 34, 28, 3, 34, 32, 1, 30, 30,
            18, 20, 30, 3, 20, 28, 2, 15, 26,
            5, 105, 20, 2, 115, 22, 2, 116, 22, // version 42
            10, 75, 32, 1, 73, 32, 0, 0, 0,
            16, 38, 32, 1, 27, 28, 0, 0, 0,
            22, 19, 28, 2, 16, 30, 1, 19, 30,
            6, 147, 28, 1, 146, 28, 0, 0, 0, // version 43
            11, 66, 28, 2, 65, 30, 0, 0, 0,
            18, 33, 28, 2, 33, 30, 0, 0, 0,
            22, 21, 32, 1, 28, 30, 0, 0, 0,
            6, 116, 22, 3, 125, 24, 0, 0, 0, // version 44
            11, 75, 32, 1, 68, 30, 0, 0, 0,
            13, 35, 28, 6, 34, 32, 1, 30, 30,
            23, 21, 32, 1, 26, 30, 0, 0, 0,
            7, 105, 20, 4, 95, 18, 0, 0, 0, // version 45
            12, 67, 28, 1, 63, 30, 1, 62, 32,
            21, 31, 26, 2, 33, 32, 0, 0, 0,
            23, 21, 32, 2, 24, 30, 0, 0, 0,
            10, 116, 22, 0, 0, 0, 0, 0, 0, // version 46
            12, 74, 32, 1, 78, 30, 0, 0, 0,
            18, 37, 32, 1, 39, 30, 1, 41, 28,
            25, 21, 32, 1, 27, 28, 0, 0, 0,
            5, 126, 24, 4, 115, 22, 1, 114, 22, // version 47
            12, 67, 28, 2, 66, 32, 1, 68, 30,
            21, 35, 30, 1, 39, 30, 0, 0, 0,
            26, 21, 32, 1, 28, 28, 0, 0, 0,
            9, 126, 24, 1, 117, 22, 0, 0, 0, // version 48
            13, 75, 32, 1, 68, 30, 0, 0, 0,
            20, 35, 30, 3, 35, 28, 0, 0, 0,
            27, 21, 32, 1, 28, 30, 0, 0, 0,
            9, 126, 24, 1, 137, 26, 0, 0, 0, // version 49
            13, 71, 30, 2, 68, 32, 0, 0, 0,
            20, 37, 32, 1, 39, 28, 1, 38, 28,
            24, 20, 32, 5, 25, 28, 0, 0, 0,
            8, 147, 28, 1, 141, 28, 0, 0, 0, // version 50
            10, 73, 32, 4, 74, 30, 1, 73, 30,
            16, 36, 32, 6, 39, 30, 1, 37, 30,
            27, 21, 32, 3, 20, 26, 0, 0, 0,
            9, 137, 26, 1, 135, 26, 0, 0, 0, // version 51
            12, 70, 30, 4, 75, 32, 0, 0, 0,
            24, 35, 30, 1, 40, 28, 0, 0, 0,
            23, 20, 32, 8, 24, 30, 0, 0, 0,
            14, 95, 18, 1, 86, 18, 0, 0, 0, // version 52
            13, 73, 32, 3, 77, 30, 0, 0, 0,
            24, 35, 30, 2, 35, 28, 0, 0, 0,
            26, 21, 32, 5, 21, 30, 1, 23, 30,
            9, 147, 28, 1, 142, 28, 0, 0, 0, // version 53
            10, 73, 30, 6, 70, 32, 1, 71, 32,
            25, 35, 30, 2, 34, 26, 0, 0, 0,
            29, 21, 32, 4, 22, 30, 0, 0, 0,
            11, 126, 24, 1, 131, 24, 0, 0, 0, // version 54
            16, 74, 32, 1, 79, 30, 0, 0, 0,
            25, 38, 32, 1, 25, 30, 0, 0, 0,
            33, 21, 32, 1, 28, 28, 0, 0, 0,
            14, 105, 20, 1, 99, 18, 0, 0, 0, // version 55
            19, 65, 28, 1, 72, 28, 0, 0, 0,
            24, 37, 32, 2, 40, 30, 1, 41, 30,
            31, 21, 32, 4, 24, 32, 0, 0, 0,
            10, 147, 28, 1, 151, 28, 0, 0, 0, // version 56
            15, 71, 30, 3, 71, 32, 1, 73, 32,
            24, 37, 32, 3, 38, 30, 1, 39, 30,
            36, 19, 30, 3, 29, 26, 0, 0, 0,
            15, 105, 20, 1, 99, 18, 0, 0, 0, // version 57
            19, 70, 30, 1, 64, 28, 0, 0, 0,
            27, 38, 32, 2, 25, 26, 0, 0, 0,
            38, 20, 30, 2, 18, 28, 0, 0, 0,
            14, 105, 20, 1, 113, 22, 1, 114, 22, // version 58
            17, 67, 30, 3, 92, 32, 0, 0, 0,
            30, 35, 30, 1, 41, 30, 0, 0, 0,
            36, 21, 32, 1, 26, 30, 1, 27, 30,
            11, 146, 28, 1, 146, 26, 0, 0, 0, // version 59
            20, 70, 30, 1, 60, 26, 0, 0, 0,
            29, 38, 32, 1, 24, 32, 0, 0, 0,
            40, 20, 30, 2, 17, 26, 0, 0, 0,
            3, 137, 26, 1, 136, 26, 10, 126, 24, // version 60
            22, 65, 28, 1, 75, 30, 0, 0, 0,
            30, 37, 32, 1, 51, 30, 0, 0, 0,
            42, 20, 30, 1, 21, 30, 0, 0, 0,
            12, 126, 24, 2, 118, 22, 1, 116, 22, // version 61
            19, 74, 32, 1, 74, 30, 1, 72, 28,
            30, 38, 32, 2, 29, 30, 0, 0, 0,
            39, 20, 32, 2, 37, 26, 1, 38, 26,
            12, 126, 24, 3, 136, 26, 0, 0, 0, // version 62
            21, 70, 30, 2, 65, 28, 0, 0, 0,
            34, 35, 30, 1, 44, 32, 0, 0, 0,
            42, 20, 30, 2, 19, 28, 2, 18, 28,
            12, 126, 24, 3, 117, 22, 1, 116, 22, // version 63
            25, 61, 26, 2, 62, 28, 0, 0, 0,
            34, 35, 30, 1, 40, 32, 1, 41, 32,
            45, 20, 30, 1, 20, 32, 1, 21, 32,
            15, 105, 20, 2, 115, 22, 2, 116, 22, // version 64
            25, 65, 28, 1, 72, 28, 0, 0, 0,
            18, 35, 30, 17, 37, 32, 1, 50, 32,
            42, 20, 30, 6, 19, 28, 1, 15, 28,
            19, 105, 20, 1, 101, 20, 0, 0, 0, // version 65
            33, 51, 22, 1, 65, 22, 0, 0, 0,
            40, 33, 28, 1, 28, 28, 0, 0, 0,
            49, 20, 30, 1, 18, 28, 0, 0, 0,
            18, 105, 20, 2, 117, 22, 0, 0, 0, // version 66
            26, 65, 28, 1, 80, 30, 0, 0, 0,
            35, 35, 30, 3, 35, 28, 1, 36, 28,
            52, 18, 28, 2, 38, 30, 0, 0, 0,
            26, 84, 16, 0, 0, 0, 0, 0, 0, // version 67
            26, 70, 30, 0, 0, 0, 0, 0, 0,
            45, 31, 26, 1, 9, 26, 0, 0, 0,
            52, 20, 30, 0, 0, 0, 0, 0, 0,
            16, 126, 24, 1, 114, 22, 1, 115, 22, // version 68
            23, 70, 30, 3, 65, 28, 1, 66, 28,
            40, 35, 30, 1, 43, 30, 0, 0, 0,
            46, 20, 30, 7, 19, 28, 1, 16, 28,
            19, 116, 22, 1, 105, 22, 0, 0, 0, // version 69
            20, 70, 30, 7, 66, 28, 1, 63, 28,
            40, 35, 30, 1, 42, 32, 1, 43, 32,
            54, 20, 30, 1, 19, 30, 0, 0, 0,
            17, 126, 24, 2, 115, 22, 0, 0, 0, // version 70
            24, 70, 30, 4, 74, 32, 0, 0, 0,
            48, 31, 26, 2, 18, 26, 0, 0, 0,
            54, 19, 28, 6, 15, 26, 1, 14, 26,
            29, 84, 16, 0, 0, 0, 0, 0, 0, // version 71
            29, 70, 30, 0, 0, 0, 0, 0, 0,
            6, 34, 30, 3, 36, 30, 38, 33, 28,
            58, 20, 30, 0, 0, 0, 0, 0, 0,
            16, 147, 28, 1, 149, 28, 0, 0, 0, // version 72
            31, 66, 28, 1, 37, 26, 0, 0, 0,
            48, 33, 28, 1, 23, 26, 0, 0, 0,
            53, 20, 30, 6, 19, 28, 1, 17, 28,
            20, 115, 22, 2, 134, 24, 0, 0, 0, // verdion 73
            29, 66, 28, 2, 56, 26, 2, 57, 26,
            45, 36, 30, 2, 15, 28, 0, 0, 0,
            59, 20, 30, 2, 21, 32, 0, 0, 0,
            17, 147, 28, 1, 134, 26, 0, 0, 0, // version 74
            26, 70, 30, 5, 75, 32, 0, 0, 0,
            47, 35, 30, 1, 48, 32, 0, 0, 0,
            64, 18, 28, 2, 33, 30, 1, 35, 30,
            22, 115, 22, 1, 133, 24, 0, 0, 0, // version 75
            33, 65, 28, 1, 74, 28, 0, 0, 0,
            43, 36, 30, 5, 27, 28, 1, 30, 28,
            57, 20, 30, 5, 21, 32, 1, 24, 32,
            18, 136, 26, 2, 142, 26, 0, 0, 0, // version 76
            33, 66, 28, 2, 49, 26, 0, 0, 0,
            48, 35, 30, 2, 38, 28, 0, 0, 0,
            64, 20, 30, 1, 20, 32, 0, 0, 0,
            19, 126, 24, 2, 135, 26, 1, 136, 26, // version 77
            32, 66, 28, 2, 55, 26, 2, 56, 26,
            49, 36, 30, 2, 18, 32, 0, 0, 0,
            65, 18, 28, 5, 27, 30, 1, 29, 30,
            20, 137, 26, 1, 130, 26, 0, 0, 0, // version 78
            30, 75, 32, 2, 71, 32, 0, 0, 0,
            46, 35, 30, 6, 39, 32, 0, 0, 0,
            3, 12, 30, 70, 19, 28, 0, 0, 0,
            20, 147, 28, 0, 0, 0, 0, 0, 0, // version 79
            35, 70, 30, 0, 0, 0, 0, 0, 0,
            49, 35, 30, 5, 35, 28, 0, 0, 0,
            70, 20, 30, 0, 0, 0, 0, 0, 0,
            21, 136, 26, 1, 155, 28, 0, 0, 0, // version 80
            34, 70, 30, 1, 64, 28, 1, 65, 28,
            54, 35, 30, 1, 45, 30, 0, 0, 0,
            68, 20, 30, 3, 18, 28, 1, 19, 28,
            19, 126, 24, 5, 115, 22, 1, 114, 22, // version 81
            33, 70, 30, 3, 65, 28, 1, 64, 28,
            52, 35, 30, 3, 41, 32, 1, 40, 32,
            67, 20, 30, 5, 21, 32, 1, 24, 32,
            2, 150, 28, 21, 136, 26, 0, 0, 0, // version 82
            32, 70, 30, 6, 65, 28, 0, 0, 0,
            52, 38, 32, 2, 27, 32, 0, 0, 0,
            73, 20, 30, 2, 22, 32, 0, 0, 0,
            21, 126, 24, 4, 136, 26, 0, 0, 0, // version 83
            30, 74, 32, 6, 73, 30, 0, 0, 0,
            54, 35, 30, 4, 40, 32, 0, 0, 0,
            75, 20, 30, 1, 20, 28, 0, 0, 0,
            30, 105, 20, 1, 114, 22, 0, 0, 0, // version 84
            3, 45, 22, 55, 47, 20, 0, 0, 0,
            2, 26, 26, 62, 33, 28, 0, 0, 0,
            79, 18, 28, 4, 33, 30, 0, 0, 0  };

        #endregion

        private int optionEccLevel;
        private int optionSymbolSize;

        public HanXinEncoder(Symbology symbology, string barcodeMessage, int optionSymbolSize, int optionEccLevel, int eci)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
            this.optionEccLevel = optionEccLevel;
            this.optionSymbolSize = optionSymbolSize;
            this.eci = eci;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            barcodeData = MessagePreProcessor.TildeParser(barcodeMessage);
            HanXin();
            return Symbol;
        }

        private void HanXin()
        {
            //int estimateBinaryLength;
            int eccLevel = optionEccLevel;
            int version;
            int dataCodewords = 0;
            int symbolSize;
            int codewords;
            int bitmask;
            byte[] datastream;
            byte[] fullstream;
            byte[] picketFence;
            byte[] symbolGrid;
            char[] functionInformation = new char[36];
            byte[] functionCodeword = new byte[3];
            byte[] functionEcc = new byte[4];
            int inputLength = barcodeData.Length;
            char[] gbData; // = new char[(inputLength + 1) * 2];
            List<char> gbDataList = new List<char>();
            char[] mode = new char[inputLength + 1];
            char[] gbCharacters;
            BitVector bitStream = new BitVector();

            for (int i = 0; i < inputLength; i++)
            {
                if (barcodeData[i] <= 0x7f)
                    gbDataList.Add(barcodeData[i]);

                else
                {
                    gbCharacters = GetGB18030Character(barcodeData[i]);
                    gbDataList.Add(gbCharacters[0]);
                    if(gbCharacters[1] != '\0')
                    {
                        gbDataList.Add(gbCharacters[1]);
                        i++;
                    }
                }
            }

            gbData = gbDataList.ToArray();
            inputLength = gbData.Length;
            DefineModes(mode, gbData, inputLength);
            //estimateBinaryLength = CalculateBinaryLength(mode, gbdata, inputLength);
            //estimatedCodewords = estimateBinaryLength / 8;
            //if (estimateBinaryLength % 8 != 0)
                //estimatedCodewords++;

            if ((eccLevel <= 0) || (eccLevel >= 5))
                eccLevel = 1;

            HXBinary(bitStream, mode, gbData, inputLength);
            codewords = bitStream.SizeInBits / 8;
            if (bitStream.SizeInBits % 8 != 0)
                codewords++;

            version = 85;
            for (int i = 84; i > 0; i--)
            {
                switch (eccLevel)
                {
                    case 1:
                        if (DataCodewordsL1[i - 1] > codewords)
                        {
                            version = i;
                            dataCodewords = DataCodewordsL1[i - 1];
                        }
                        break;

                    case 2:
                        if (DataCodewordsL2[i - 1] > codewords)
                        {
                            version = i;
                            dataCodewords = DataCodewordsL2[i - 1];
                        }
                        break;

                    case 3:
                        if (DataCodewordsL3[i - 1] > codewords)
                        {
                            version = i;
                            dataCodewords = DataCodewordsL3[i - 1];
                        }
                        break;

                    case 4:
                        if (DataCodewordsL4[i - 1] > codewords)
                        {
                            version = i;
                            dataCodewords = DataCodewordsL4[i - 1];
                        }
                        break;
                }
            }

            if (version == 85)
                throw new InvalidDataLengthException("Han Xin: Input data too long for selected error correction.");

            if ((optionSymbolSize < 0) || (optionSymbolSize > 84))
                optionSymbolSize = 0;

            if (optionSymbolSize > version)
                version = optionSymbolSize;

            // If there is spare capacity, increase the level of ECC.
            if ((eccLevel == 1) && (codewords < DataCodewordsL2[version - 1]))
            {
                eccLevel = 2;
                dataCodewords = DataCodewordsL2[version - 1];
            }

            if ((eccLevel == 2) && (codewords < DataCodewordsL3[version - 1]))
            {
                eccLevel = 3;
                dataCodewords = DataCodewordsL3[version - 1];
            }

            if ((eccLevel == 3) && (codewords < DataCodewordsL4[version - 1]))
            {
                eccLevel = 4;
                dataCodewords = DataCodewordsL4[version - 1];
            }

            symbolSize = (version * 2) + 21;

            datastream = new byte[dataCodewords];
            Array.Copy(bitStream.ToByteArray(), datastream, bitStream.SizeInBytes);
            /*for (int i = 0; i < bitStream.SizeInBits; i++)
            {
                if (bitStream[i] == 1)
                    datastream[i / 8] += (byte)(0x80 >> (i % 8));
            }*/
            fullstream = new byte[TotalCodewords[version - 1]];
            picketFence = new byte[TotalCodewords[version - 1]];
            symbolGrid = new byte[symbolSize * symbolSize];

            SetupGrid(symbolGrid, symbolSize, version);
            AddErrorCorrection(fullstream, datastream, version, eccLevel);
            BuildPicketFence(fullstream, picketFence, TotalCodewords[version - 1]);

            // Populate grid.
            int index = 0;
            for (int i = 0; i < (symbolSize * symbolSize); i++)
            {
                if (symbolGrid[i] == 0x00)
                {
                    if (index < (TotalCodewords[version - 1] * 8))
                    {
                        if ((picketFence[(index / 8)] & (0x80 >> (index % 8))) > 0)
                            symbolGrid[i] = 0x01;

                        index++;
                    }
                }
            }

            bitmask = ApplyBitmask(symbolGrid, symbolSize);
            // Form function information string.
            for (int i = 0; i < 34; i++)
            {
                if ((i % 2) > 0)
                    functionInformation[i] = '1';

                else
                    functionInformation[i] = '0';
            }

            functionInformation[34] = '\0';

            for (int i = 0; i < 8; i++)
            {
                if (((version + 20) & (0x80 >> i)) > 0)
                    functionInformation[i] = '1';

                else
                    functionInformation[i] = '0';
            }

            for (int i = 0; i < 2; i++)
            {
                if ((eccLevel & (0x02 >> i)) > 0)
                    functionInformation[i + 8] = '1';

                else
                    functionInformation[i + 8] = '0';
            }

            for (int i = 0; i < 2; i++)
            {
                if ((bitmask & (0x02 >> i)) > 0)
                    functionInformation[i + 10] = '1';

                else
                    functionInformation[i + 10] = '0';
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (functionInformation[(i * 4) + j] == '1')
                        functionCodeword[i] += (byte)(0x08 >> j);
                }
            }

            ReedSolomon.RSInitialise(0x13, 4, 1);
            ReedSolomon.RSEncode(3, functionCodeword, functionEcc);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if ((functionEcc[3 - i] & (0x08 >> j)) > 0)
                        functionInformation[(i * 4) + j + 12] = '1';

                    else
                        functionInformation[(i * 4) + j + 12] = '0';
                }
            }

            // Add function information to symbol.
            for (int i = 0; i < 9; i++)
            {
                if (functionInformation[i] == '1')
                {
                    symbolGrid[(8 * symbolSize) + i] = 0x01;
                    symbolGrid[((symbolSize - 8 - 1) * symbolSize) + (symbolSize - i - 1)] = 0x01;
                }

                if (functionInformation[i + 8] == '1')
                {
                    symbolGrid[((8 - i) * symbolSize) + 8] = 0x01;
                    symbolGrid[((symbolSize - 8 - 1 + i) * symbolSize) + (symbolSize - 8 - 1)] = 0x01;
                }

                if (functionInformation[i + 17] == '1')
                {
                    symbolGrid[(i * symbolSize) + (symbolSize - 1 - 8)] = 0x01;
                    symbolGrid[((symbolSize - 1 - i) * symbolSize) + 8] = 0x01;
                }

                if (functionInformation[i + 25] == '1')
                {
                    symbolGrid[(8 * symbolSize) + (symbolSize - 1 - 8 + i)] = 0x01;
                    symbolGrid[((symbolSize - 1 - 8) * symbolSize) + (8 - i)] = 0x01;
                }
            }

            // Build the symbol.
            byte[] rowData;
            for (int i = 0; i < symbolSize; i++)
            {
                rowData = new byte[symbolSize];
                for (int j = 0; j < symbolSize; j++)
                {
                    if ((symbolGrid[(i * symbolSize) + j] & 0x01) > 0)
                        rowData[j] = 1;
                }

                SymbolData symbolData = new SymbolData(rowData, 1.0f);
                Symbol.Insert(i, symbolData);
            }
        }

        private static void DefineModes(char[] mode, char[] gbData, int length)
        {
            int i = 0;
            char lastmode = 't';
            bool done;

            do
            {
                done = false;
                if (IsRegion1(gbData[i]))
                {
                    mode[i] = '1';
                    done = true;
                    i++;
                }

                if ((!done) && (IsRegion2(gbData[i])))
                {
                    mode[i] = '2';
                    done = true;
                    i++;
                }

                if ((!done) && (IsDoubleByte(gbData[i])))
                {
                    mode[i] = 'd';
                    done = true;
                    i++;
                }

                if ((!done) && (i < length - 1))
                {
                    if (IsFourByte(gbData[i], gbData[i + 1]))
                    {
                        mode[i] = 'f';
                        mode[i + 1] = 'f';
                        done = true;
                        i += 2;
                    }
                }

                if (!done)
                {
                    if (Char.IsDigit(gbData[i]))
                    {
                        mode[i] = 'n';
                        if (lastmode != 'n')
                            lastmode = 'n';
                    }

                    else
                    {
                        if ((gbData[i] <= 127) && ((gbData[i] <= 27) || (gbData[i] >= 32)))
                        {
                            mode[i] = 't';
                            if (lastmode != 't')
                                lastmode = 't';
                        }

                        else
                        {
                            mode[i] = 'b';
                            if (lastmode != 'b')
                                lastmode = 'b';
                        }
                    }
                    i++;
                }
            } while (i < length);
            //mode[length] = '\0';
        }

        /*private int CalculateBinaryLength(char[] mode, char[] gbData, int length)
        {
            int i = 0;
            char lastMode = 't';
            int estimatedBinaryLength = 0;
            int submode = 1;

            if (eci != 3)
                estimatedBinaryLength += 12;

            do
            {
                switch (mode[i])
                {
                    case 'n':
                        if (lastMode != 'n')
                        {
                            estimatedBinaryLength += 14;
                            lastMode = 'n';
                        }

                        estimatedBinaryLength += 4;
                        break;

                    case 't':
                        if (lastMode != 't')
                        {
                            estimatedBinaryLength += 10;
                            lastMode = 't';
                            submode = 1;
                        }

                        if (GetSubmode((char)gbData[i]) != submode)
                        {
                            estimatedBinaryLength += 6;
                            submode = GetSubmode(gbData[i]);
                        }

                        estimatedBinaryLength += 6;
                        break;

                    case 'b':
                        if (lastMode != 'b')
                        {
                            estimatedBinaryLength += 17;
                            lastMode = 'b';
                        }

                        estimatedBinaryLength += 8;
                        break;

                    case '1':
                        if (lastMode != '1')
                        {
                            estimatedBinaryLength += 16;
                            lastMode = '1';
                        }

                        estimatedBinaryLength += 12;
                        break;

                    case '2':
                        if (lastMode != '2')
                        {
                            estimatedBinaryLength += 16;
                            lastMode = '2';
                        }

                        estimatedBinaryLength += 12;
                        break;

                    case 'd':
                        if (lastMode != 'd')
                        {
                            estimatedBinaryLength += 16;
                            lastMode = 'd';
                        }

                        estimatedBinaryLength += 15;
                        break;

                    case 'f':
                        if (lastMode != 'f')
                        {
                            estimatedBinaryLength += 4;
                            lastMode = 'f';
                        }

                        estimatedBinaryLength += 21;
                        i++;
                        break;
                }

                i++;
            } while (i < length);

            return estimatedBinaryLength;
        }*/

        // Convert input data to bit stream.
        private void HXBinary(BitVector bitStream, char[] mode, char[] gbData, int length)
        {
            int blockLength;
            int position = 0;
            int count = 0;
            int i, encodingValue;
            int firstByte, secondByte;
            int thirdByte, fourthByte;
            int glyph;
            int submode;

            if (eci != 3)
            {
                bitStream.AppendBits(8, 4);    // 1000.
                bitStream.AppendBits(eci, 8);
            }

            do
            {
                blockLength = 0;
                do
                {
                    blockLength++;
                } while (mode[position + blockLength] == mode[position]);

                switch (mode[position])
                {
                    case 'n':
                        // Numeric mode
                        // Mode indicator
                        bitStream.AppendBits(1, 4);    // 0001.
                        i = 0;
                        while (i < blockLength)
                        {
                            int first = 0, second = 0, third = 0;

                            first = CharacterSets.NumberOnlySet.IndexOf(gbData[position + i]);
                            count = 1;
                            encodingValue = first;

                            if (i + 1 < blockLength && mode[position + i + 1] == 'n')
                            {
                                second = CharacterSets.NumberOnlySet.IndexOf(gbData[position + i + 1]);
                                count = 2;
                                encodingValue = (encodingValue * 10) + second;

                                if (i + 2 < blockLength && mode[position + i + 2] == 'n')
                                {
                                    third = CharacterSets.NumberOnlySet.IndexOf(gbData[position + i + 2]);
                                    count = 3;
                                    encodingValue = (encodingValue * 10) + third;
                                }
                            }

                            bitStream.AppendBits(encodingValue, 10);
                            /*for (p = 0; p < 10; p++)
                            {
                                if (encoding_value & (0x200 >> p))
                                {
                                    strcat(binary, "1");
                                }
                                else
                                {
                                    strcat(binary, "0");
                                }
                            }*/
                            i += count;
                        }

                        // Mode terminator depends on number of characters in last group (Table 2).
                        bitStream.AppendBits(1020 + count, 10);
                        /* switch (count)
                         {
                             case 1:
                                 strcat(binary, "1111111101");
                                 break;
                             case 2:
                                 strcat(binary, "1111111110");
                                 break;
                             case 3:
                                 strcat(binary, "1111111111");
                                 break;
                         }*/
                        break;

                    case 't':
                        // Text mode.
                        if (position != 0)
                        {
                            // Mode indicator.
                            bitStream.AppendBits(2, 4);    // 0010.
                            //strcat(binary, "0010");
                        }

                        submode = 1;
                        i = 0;
                        while (i < blockLength)
                        {

                            if (GetSubmode(gbData[i + position]) != submode)
                            {
                                // Change submode.
                                bitStream.AppendBits(62, 6);   // 111110.
                                submode = GetSubmode(gbData[i + position]);
                            }

                            if (submode == 1)
                                encodingValue = LookupText1(gbData[i + position]);

                            else
                                encodingValue = LookupText2(gbData[i + position]);

                            bitStream.AppendBits(encodingValue, 6);
                            /*for (p = 0; p < 6; p++)
                            {
                                if (encoding_value & (0x20 >> p))
                                {
                                    strcat(binary, "1");
                                }
                                else
                                {
                                    strcat(binary, "0");
                                }
                            }*/
                            i++;
                        }

                        // Terminator.
                        bitStream.AppendBits(63, 6);   // 111111.
                        break;

                    case 'b':
                        // Binary Mode.
                        // Mode indicator.
                        bitStream.AppendBits(3, 4);    // 0011
                        // Count indicator.
                        bitStream.AppendBits(blockLength, 13);
                        /*for (p = 0; p < 13; p++)
                        {
                            if (block_length & (0x1000 >> p))
                            {
                                strcat(binary, "1");
                            }
                            else
                            {
                                strcat(binary, "0");
                            }
                        }*/
                        i = 0;
                        while (i < blockLength)
                        {

                            // 8-bit bytes with no conversion.
                            bitStream.AppendBits(gbData[i + position], 8);
                            /*for (p = 0; p < 8; p++)
                            {
                                if (gbdata[i + position] & (0x80 >> p))
                                {
                                    strcat(binary, "1");
                                }
                                else
                                {
                                    strcat(binary, "0");
                                }
                            }*/
                            i++;
                        }

                        break;

                    case '1':
                        // Region 1 encoding.
                        // Mode indicator.
                        bitStream.AppendBits(4, 4);    // 0100.
                        i = 0;
                        while (i < blockLength)
                        {
                            firstByte = (gbData[i + position] & 0xff00) >> 8;
                            secondByte = gbData[i + position] & 0xff;

                            // Subset 1.
                            glyph = (0x5e * (firstByte - 0xb0)) + (secondByte - 0xa1);

                            // Subset 2.
                            if ((firstByte >= 0xa1) && (firstByte <= 0xa3))
                            {
                                if ((secondByte >= 0xa1) && (secondByte <= 0xfe))
                                    glyph = (0x5e * firstByte - 0xa1) + (secondByte - 0xa1) + 0xeb0;
                            }

                            // Subset 3.
                            if ((gbData[i + position] >= 0xa8a1) && (gbData[i + position] <= 0xa8c0))
                                glyph = (secondByte - 0xa1) + 0xfca;

                            bitStream.AppendBits(glyph, 12);
                            /*for (p = 0; p < 12; p++)
                            {
                                if (glyph & (0x800 >> p))
                                {
                                    strcat(binary, "1");
                                }
                                else
                                {
                                    strcat(binary, "0");
                                }
                            }*/

                            i++;
                        }

                        // Terminator.
                        bitStream.AppendBits(4095, 12);    // 111111111111.
                        break;

                    case '2':
                        // Region 2 encoding.
                        // Mode indicator.
                        bitStream.AppendBits(5, 4);    // 0101.
                        i = 0;
                        while (i < blockLength)
                        {
                            firstByte = (gbData[i + position] & 0xff00) >> 8;
                            secondByte = gbData[i + position] & 0xff;

                            glyph = (0x5e * (firstByte - 0xd8)) + (secondByte - 0xa1);
                            bitStream.AppendBits(glyph, 12);
                            /* for (p = 0; p < 12; p++)
                             {
                                 if (glyph & (0x800 >> p))
                                 {
                                     strcat(binary, "1");
                                 }
                                 else
                                 {
                                     strcat(binary, "0");
                                 }
                             }*/

                            i++;
                        }

                        // Terminator.
                        bitStream.AppendBits(4095, 12);    // 111111111111.
                        break;

                    case 'd':
                        // Double byte encoding.
                        // Mode indicator.
                        bitStream.AppendBits(6, 4);    // 0110.
                        i = 0;
                        while (i < blockLength)
                        {
                            firstByte = (gbData[i + position] & 0xff00) >> 8;
                            secondByte = gbData[i + position] & 0xff;

                            if (secondByte <= 0x7e)
                                glyph = (0xbe * (firstByte - 0x81)) + (secondByte - 0x40);

                            else
                                glyph = (0xbe * (firstByte - 0x81)) + (secondByte - 0x41);

                            bitStream.AppendBits(glyph, 15);
                            /*for (p = 0; p < 15; p++)
                            {
                                if (glyph & (0x4000 >> p))
                                {
                                    strcat(binary, "1");
                                }
                                else
                                {
                                    strcat(binary, "0");
                                }
                            }*/

                            i++;
                        }

                        // Terminator.
                        bitStream.AppendBits(32767, 15);   // 111111111111111.
                        // Terminator sequence of length 12 is a mistake - confirmed by Wang Yi.
                        break;

                    case 'f':
                        // Four-byte encoding
                        i = 0;
                        while (i < blockLength)
                        {

                            // Mode indicator.
                            bitStream.AppendBits(7, 4);    // 0111;

                            firstByte = (gbData[i + position] & 0xff00) >> 8;
                            secondByte = gbData[i + position] & 0xff;
                            thirdByte = (gbData[i + position + 1] & 0xff00) >> 8;
                            fourthByte = gbData[i + position + 1] & 0xff;

                            glyph = (0x3138 * (firstByte - 0x81)) + (0x04ec * (secondByte - 0x30)) +
                                    (0x0a * (thirdByte - 0x81)) + (fourthByte - 0x30);

                            bitStream.AppendBits(glyph, 15);
                            /*for (p = 0; p < 15; p++)
                            {
                                if (glyph & (0x4000 >> p))
                                {
                                    strcat(binary, "1");
                                }
                                else
                                {
                                    strcat(binary, "0");
                                }
                            }*/
                            i += 2;
                        }

                        // No terminator!!!
                        break;

                }

                position += blockLength;

            } while (position < length);
        }

        private static bool IsRegion1(char glyph)
        {
            int firstByte, secondByte;
            bool valid = false;

            firstByte = (glyph & 0xff00) >> 8;
            secondByte = glyph & 0xff;

            if ((firstByte >= 0xb0) && (firstByte <= 0xd7))
            {
                if ((secondByte >= 0xa1) && (secondByte <= 0xfe))
                    valid = true;
            }

            if ((firstByte >= 0xa1) && (firstByte <= 0xa3))
            {
                if ((secondByte >= 0xa1) && (secondByte <= 0xfe))
                    valid = true;
            }

            if ((glyph >= 0xa8a1) && (glyph <= 0xa8c0))
                valid = true;

            return valid;
        }

        private static bool IsRegion2(char glyph)
        {
            int firstByte, secondByte;
            bool valid = false;

            firstByte = (glyph & 0xff00) >> 8;
            secondByte = glyph & 0xff;

            if ((firstByte >= 0xd8) && (firstByte <= 0xf7))
            {
                if ((secondByte >= 0xa1) && (secondByte <= 0xfe))
                    valid = true;
            }

            return valid;
        }

        private static bool IsDoubleByte(char glyph)
        {
            int firstByte, secondByte;
            bool valid = false;

            firstByte = (glyph & 0xff00) >> 8;
            secondByte = glyph & 0xff;

            if ((firstByte >= 0x81) && (firstByte <= 0xfe))
            {
                if ((secondByte >= 0x40) && (secondByte <= 0x7e))
                    valid = true;

                if ((secondByte >= 0x80) && (secondByte <= 0xfe))
                    valid = true;
            }

            return valid;
        }

        private static bool IsFourByte(char glyph1, char glyph2)
        {
            int firstByte, secondByte;
            int thirdByte, fourthByte;
            bool valid = false;

            firstByte = (glyph1 & 0xff00) >> 8;
            secondByte = glyph1 & 0xff;
            thirdByte = (glyph2 & 0xff00) >> 8;
            fourthByte = glyph2 & 0xff;

            if ((firstByte >= 0x81) && (firstByte <= 0xfe))
            {
                if ((secondByte >= 0x30) && (secondByte <= 0x39))
                {
                    if ((thirdByte >= 0x81) && (thirdByte <= 0xfe))
                    {
                        if ((fourthByte >= 0x30) && (fourthByte <= 0x39))
                            valid = true;
                    }
                }
            }

            return valid;
        }

        // Find which submode to use for a text character.
        private static int GetSubmode(char input)
        {
            int subMode = 2;

            if (Char.IsDigit(input))
                subMode = 1;

            if (Char.IsUpper(input))
                subMode = 1;

            if (Char.IsLower(input))
                subMode = 1;

            return subMode;
        }

        // Convert Text 1 sub-mode character to encoding value, as given in table 3.
        private static int LookupText1(char input)
        {
            int encodingValue = 0;

            if (Char.IsDigit(input))
                encodingValue = input - '0';

            if (Char.IsUpper(input))
                encodingValue = input - 'A' + 10;

            if (Char.IsLower(input))
                encodingValue = input - 'a' + 36;

            return encodingValue;
        }

        // Convert Text 2 sub-mode character to encoding value, as given in table 4.
        private static int LookupText2(char input)
        {
            int encodingValue = 0;

            if ((input >= 0) && (input <= 27))
                encodingValue = input;

            if ((input >= ' ') && (input <= '/'))
                encodingValue = input - ' ' + 28;

            if ((input >= '[') && (input <= 96))
                encodingValue = input - '[' + 51;

            if ((input >= '{') && (input <= 127))
                encodingValue = input - '{' + 57;

            return encodingValue;
        }

        // Put static elements in the grid */
        private static void SetupGrid(byte[] grid, int size, int version)
        {
            int i, j;

            for (i = 0; i < size; i++)
            {
                for (j = 0; j < size; j++)
                    grid[(i * size) + j] = 0;
            }

            // Add finder patterns.
            PlaceFinderTopLeft(grid, size);
            PlaceFinder(grid, size, 0, size - 7);
            PlaceFinder(grid, size, size - 7, 0);
            PlaceFinderBottomRight(grid, size);

            // Add finder pattern separator region.
            for (i = 0; i < 8; i++)
            {
                /* Top left */
                grid[(7 * size) + i] = 0x10;
                grid[(i * size) + 7] = 0x10;

                /* Top right */
                grid[(7 * size) + (size - i - 1)] = 0x10;
                grid[((size - i - 1) * size) + 7] = 0x10;

                /* Bottom left */
                grid[(i * size) + (size - 8)] = 0x10;
                grid[((size - 8) * size) + i] = 0x10;

                /* Bottom right */
                grid[((size - 8) * size) + (size - i - 1)] = 0x10;
                grid[((size - i - 1) * size) + (size - 8)] = 0x10;
            }

            /* Reserve function information region */
            for (i = 0; i < 9; i++)
            {
                /* Top left */
                grid[(8 * size) + i] = 0x10;
                grid[(i * size) + 8] = 0x10;

                /* Top right */
                grid[(8 * size) + (size - i - 1)] = 0x10;
                grid[((size - i - 1) * size) + 8] = 0x10;

                /* Bottom left */
                grid[(i * size) + (size - 9)] = 0x10;
                grid[((size - 9) * size) + i] = 0x10;

                /* Bottom right */
                grid[((size - 9) * size) + (size - i - 1)] = 0x10;
                grid[((size - i - 1) * size) + (size - 9)] = 0x10;
            }

            if (version > 3)
            {
                int k = ModuleK[version - 1];
                int r = ModuleR[version - 1];
                int m = ModuleM[version - 1];
                int x, y, row_switch, column_switch;
                int module_height, module_width;
                int mod_x, mod_y;

                // Add assistant alignment patterns to left and right.
                y = 0;
                mod_y = 0;
                do
                {
                    if (mod_y < m)
                        module_height = k;

                    else
                        module_height = r - 1;

                    if ((mod_y % 2) == 0)
                    {
                        if ((m % 2) == 1)
                            hx_plot_assistant(grid, size, 0, y);
                    }

                    else
                    {
                        if ((m % 2) == 0)
                            hx_plot_assistant(grid, size, 0, y);

                        hx_plot_assistant(grid, size, size - 1, y);
                    }

                    mod_y++;
                    y += module_height;
                } while (y < size);

                // Add assistant alignment patterns to top and bottom.
                x = (size - 1);
                mod_x = 0;
                do
                {
                    if (mod_x < m)
                        module_width = k;

                    else
                        module_width = r - 1;

                    if ((mod_x % 2) == 0)
                    {
                        if ((m % 2) == 1)
                            hx_plot_assistant(grid, size, x, (size - 1));
                    }

                    else
                    {
                        if ((m % 2) == 0)
                            hx_plot_assistant(grid, size, x, (size - 1));

                        hx_plot_assistant(grid, size, x, 0);
                    }

                    mod_x++;
                    x -= module_width;
                } while (x >= 0);

                /* Add alignment pattern */
                column_switch = 1;
                y = 0;
                mod_y = 0;
                do
                {
                    if (mod_y < m)
                        module_height = k;

                    else
                        module_height = r - 1;


                    if (column_switch == 1)
                    {
                        row_switch = 1;
                        column_switch = 0;
                    }

                    else
                    {
                        row_switch = 0;
                        column_switch = 1;
                    }

                    x = (size - 1);
                    mod_x = 0;
                    do
                    {
                        if (mod_x < m)
                            module_width = k;

                        else
                            module_width = r - 1;

                        if (row_switch == 1)
                        {
                            if (!(y == 0 && x == (size - 1)))
                                hx_plot_alignment(grid, size, x, y, module_width, module_height);

                            row_switch = 0;
                        }

                        else
                            row_switch = 1;

                        mod_x++;
                        x -= module_width;
                    } while (x >= 0);

                    mod_y++;
                    y += module_height;
                } while (y < size);
            }
        }

        // Calculate error correction codes.
        private static void AddErrorCorrection(byte[] fullstream, byte[] datastream, int version, int eccLevel)
        {
            byte[] dataBlock = new byte[180];
            byte[] eccBlock = new byte[36];
            int i, j, block;
            int batchSize, dataSize, eccSize;
            int inputPosition = -1;
            int outputPosition = -1;
            int TableD1Position = ((version - 1) * 36) + ((eccLevel - 1) * 9);

            for (i = 0; i < 3; i++)
            {
                batchSize = TableD1[TableD1Position + (3 * i)];
                dataSize = TableD1[TableD1Position + (3 * i) + 1];
                eccSize = TableD1[TableD1Position + (3 * i) + 2];

                for (block = 0; block < batchSize; block++)
                {
                    for (j = 0; j < dataSize; j++)
                    {
                        inputPosition++;
                        outputPosition++;
                        dataBlock[j] = datastream[inputPosition];
                        fullstream[outputPosition] = datastream[inputPosition];
                    }

                    ReedSolomon.RSInitialise(0x163, eccSize, 1);    // x^8 + x^6 + x^5 + x + 1 = 0
                    ReedSolomon.RSEncode(dataSize, dataBlock, eccBlock);
                    for (j = 0; j < eccSize; j++)
                    {
                        outputPosition++;
                        fullstream[outputPosition] = eccBlock[eccSize - j - 1];
                    }
                }
            }
        }

        // Rearrange data in batches of 13 codewords (section 5.8.2) */
        private static void BuildPicketFence(byte[] fullstream, byte[] picketFence, int streamSize)
        {
            int outputPosition = 0;

            for (int start = 0; start < 13; start++)
            {
                for (int i = start; i < streamSize; i += 13)
                {
                    if (i < streamSize)
                    {
                        picketFence[outputPosition] = fullstream[i];
                        outputPosition++;
                    }
                }
            }
        }

        // Apply the four possible bitmasks for evaluation.
        private static int ApplyBitmask(byte[] grid, int size)
        {
            int x, y;
            int i, j;
            int pattern;
            int[] penalty = new int[4];
            int best_pattern, best_val;
            int bit;
            byte p;
            byte[] mask = new byte[size * size];
            byte[] eval = new byte[size * size];
        
            // Perform data masking
            for (x = 0; x < size; x++)
            {
                for (y = 0; y < size; y++)
                {
                    mask[(y * size) + x] = 0x00;
                    j = x + 1;
                    i = y + 1;

                    if ((grid[(y * size) + x] & 0xf0) == 0)
                    {
                        if ((i + j) % 2 == 0)
                            mask[(y * size) + x] += 0x02;

                        if ((((i + j) % 3) + (j % 3)) % 2 == 0)
                            mask[(y * size) + x] += 0x04;

                        if (((i % j) + (j % i) + (i % 3) + (j % 3)) % 2 == 0)
                            mask[(y * size) + x] += 0x08;
                    }
                }
            }

            // Apply data masks to grid, result in eval
            for (x = 0; x < size; x++) {
                for (y = 0; y < size; y++) {
                    if ((grid[(y * size) + x] & 0x01) > 0)
                        p = 0xff;

                    else
                        p = 0x00;

                    eval[(y * size) + x] = (byte)(mask[(y * size) + x] ^ p);
                }
            }

            // Evaluate result.
            for (pattern = 0; pattern < 4; pattern++)
                penalty[pattern] = Evaluate(eval, size, pattern);

            best_pattern = 0;
            best_val = penalty[0];
            for (pattern = 1; pattern < 4; pattern++)
            {
                if (penalty[pattern] < best_val)
                {
                    best_pattern = pattern;
                    best_val = penalty[pattern];
                }
            }

            // Apply mask.
            for (x = 0; x < size; x++)
            {
                for (y = 0; y < size; y++)
                {
                    bit = 0;
                    switch (best_pattern)
                    {
                        case 0:
                            if ((mask[(y * size) + x] & 0x01) > 0)
                                bit = 1;
                            break;

                        case 1:
                            if ((mask[(y * size) + x] & 0x02) > 0)
                                bit = 1;
                            break;

                        case 2:
                            if ((mask[(y * size) + x] & 0x04) > 0)
                                bit = 1;
                            break;

                        case 3: if ((mask[(y * size) + x] & 0x08) > 0)
                                bit = 1;
                           break;
                    }

                    if (bit == 1)
                    {
                        if ((grid[(y * size) + x] & 0x01) > 0)
                            grid[(y * size) + x] = 0x00;

                        else
                            grid[(y * size) + x] = 0x01;
                    }
                }
            }

            return best_pattern;
        }

        // Evaluate a bitmask according to table 9.
        private static int Evaluate(byte[] evaluation, int size, int pattern)
        {
            int x, y, block, weight;
            int result = 0;
            byte state;
            int p;
            int a, b, afterCount, beforeCount;
            byte[] local = new byte[size * size];
        
            /* All four bitmask variants have been encoded in the 4 bits of the bytes
             * that make up the grid array. select them for evaluation according to the
             * desired pattern.*/
            for (x = 0; x < size; x++)
            {
                for (y = 0; y < size; y++)
                {
                    if ((evaluation[(y * size) + x] & (0x01 << pattern)) != 0)
                        local[(y * size) + x] = 1;
                    
                    else
                        local[(y * size) + x] = 0;
                }
            }

            // Test 1: 1:1:1:1:3  or 3:1:1:1:1 ratio pattern in row/column.
            // Vertical.
            for (x = 0; x < size; x++)
            {
                for (y = 0; y < (size - 7); y++)
                {
                    p = 0;
                    for (weight = 0; weight < 7; weight++)
                    {
                        if (local[((y + weight) * size) + x] == 1)
                            p += (0x40 >> weight);
                    }

                    if ((p == 0x57) || (p == 0x75))
                    {
                        // Pattern found, check before and after.
                        beforeCount = 0;
                        for (b = (y - 3); b < y; b++)
                        {
                            if (b < 0)
                                beforeCount++;

                            else
                            {
                                if (local[(b * size) + x] == 0)
                                    beforeCount++;

                                else
                                    beforeCount = 0;
                            }
                        }

                        afterCount = 0;
                        for (a = (y + 7); a <= (y + 9); a++)
                        {
                            if (a >= size)
                                afterCount++;

                            else
                            {
                                if (local[(a * size) + x] == 0)
                                    afterCount++;

                                else
                                    afterCount = 0;
                            }
                        }

                        if ((beforeCount == 3) || (afterCount == 3))    // Pattern is preceeded or followed by light area 3 modules wide.
                            result += 50;
                    }
                }
            }

            // Horizontal.
            for (y = 0; y < size; y++)
            {
                for (x = 0; x < (size - 7); x++)
                {
                    p = 0;
                    for (weight = 0; weight < 7; weight++)
                    {
                        if (local[(y * size) + x + weight] == 1)
                            p += (0x40 >> weight);
                    }

                    if ((p == 0x57) || (p == 0x75))
                    {
                        // Pattern found, check before and after.
                        beforeCount = 0;
                        for (b = (x - 3); b < x; b++)
                        {
                            if (b < 0)
                                beforeCount++;
                            
                            else
                            {
                                if (local[(y * size) + b] == 0)
                                    beforeCount++;

                                else
                                    beforeCount = 0;
                            }
                        }

                        afterCount = 0;
                        for (a = (x + 7); a <= (x + 9); a++)
                        {
                            if (a >= size)
                                afterCount++;

                            else
                            {
                                if (local[(y * size) + a] == 0)
                                    afterCount++;

                                else
                                    afterCount = 0;
                            }
                        }

                        if ((beforeCount == 3) || (afterCount == 3))    // Pattern is preceeded or followed by light area 3 modules wide.
                            result += 50;
                    }
                }
            }

            // Test 2: Adjacent modules in row/column in same colour.
            /* In AIMD-15 section 5.8.3.2 it is stated... “In Table 9 below, i refers to the row
             * position of the module.” - however i being the length of the run of the
             * same colour (i.e. "block" below) in the same fashion as ISO/IEC 18004
             * makes more sense. -- Confirmed by Wang Yi */

            // Vertical.
            for (x = 0; x < size; x++)
            {
                state = local[x];
                block = 0;
                for (y = 0; y < size; y++)
                {
                    if (local[(y * size) + x] == state)
                        block++;

                    else
                    {
                        if (block > 3)
                            result += (3 + block) * 4;

                        block = 0;
                        state = local[(y * size) + x];
                    }
                }

                if (block > 3)
                    result += (3 + block) * 4;
            }

            // Horizontal.
            for (y = 0; y < size; y++)
            {
                state = local[y * size];
                block = 0;
                for (x = 0; x < size; x++)
                {
                    if (local[(y * size) + x] == state)
                        block++;

                    else
                    {
                        if (block > 3)
                            result += (3 + block) * 4;

                        block = 0;
                        state = local[(y * size) + x];
                    }
                }

                if (block > 3)
                    result += (3 + block) * 4;
            }

            return result;
        }

        // Finder pattern for top left of symbol
        private static void PlaceFinderTopLeft(byte[] grid, int size)
        {
            int xp, yp;
            int x = 0, y = 0;

            int[] finder = {
                1, 1, 1, 1, 1, 1, 1,
                1, 0, 0, 0, 0, 0, 0,
                1, 0, 1, 1, 1, 1, 1,
                1, 0, 1, 0, 0, 0, 0,
                1, 0, 1, 0, 1, 1, 1,
                1, 0, 1, 0, 1, 1, 1,
                1, 0, 1, 0, 1, 1, 1
            };

            for (xp = 0; xp < 7; xp++)
            {
                for (yp = 0; yp < 7; yp++)
                {
                    if (finder[xp + (7 * yp)] == 1)
                        grid[((yp + y) * size) + (xp + x)] = 0x11;

                    else
                        grid[((yp + y) * size) + (xp + x)] = 0x10;
                }
            }
        }

        // Finder pattern for top right and bottom left of symbol.
        private static void PlaceFinder(byte[] grid, int size, int x, int y)
        {
            int xp, yp;

            int[] finder = {
                1, 1, 1, 1, 1, 1, 1,
                0, 0, 0, 0, 0, 0, 1,
                1, 1, 1, 1, 1, 0, 1,
                0, 0, 0, 0, 1, 0, 1,
                1, 1, 1, 0, 1, 0, 1,
                1, 1, 1, 0, 1, 0, 1,
                1, 1, 1, 0, 1, 0, 1 };

            for (xp = 0; xp < 7; xp++)
            {
                for (yp = 0; yp < 7; yp++)
                {
                    if (finder[xp + (7 * yp)] == 1)
                        grid[((yp + y) * size) + (xp + x)] = 0x11;

                    else
                        grid[((yp + y) * size) + (xp + x)] = 0x10;
                }
            }
        }

        // Finder pattern for bottom right of symbol.
        private static void PlaceFinderBottomRight(byte[] grid, int size)
        {
            int xp, yp;
            int x = size - 7, y = size - 7;

            int[] finder = {
                1, 1, 1, 0, 1, 0, 1,
                1, 1, 1, 0, 1, 0, 1,
                1, 1, 1, 0, 1, 0, 1,
                0, 0, 0, 0, 1, 0, 1,
                1, 1, 1, 1, 1, 0, 1,
                0, 0, 0, 0, 0, 0, 1,
                1, 1, 1, 1, 1, 1, 1 };

            for (xp = 0; xp < 7; xp++)
            {
                for (yp = 0; yp < 7; yp++)
                {
                    if (finder[xp + (7 * yp)] == 1)
                        grid[((yp + y) * size) + (xp + x)] = 0x11;

                    else
                        grid[((yp + y) * size) + (xp + x)] = 0x10;
                }
            }
        }

        // Plot assistant alignment patterns.
        private static void hx_plot_assistant(byte[] grid, int size, int x, int y)
        {
            SafePlot(grid, size, x - 1, y - 1, 0x10);
            SafePlot(grid, size, x, y - 1, 0x10);
            SafePlot(grid, size, x + 1, y - 1, 0x10);
            SafePlot(grid, size, x - 1, y, 0x10);
            SafePlot(grid, size, x, y, 0x11);
            SafePlot(grid, size, x + 1, y, 0x10);
            SafePlot(grid, size, x - 1, y + 1, 0x10);
            SafePlot(grid, size, x, y + 1, 0x10);
            SafePlot(grid, size, x + 1, y + 1, 0x10);
        }

        // Plot an alignment pattern around top and right of a module.
        private static void hx_plot_alignment(byte[] grid, int size, int x, int y, int w, int h)
        {
            SafePlot(grid, size, x, y, 0x11);
            SafePlot(grid, size, x - 1, y + 1, 0x10);

            for (int i = 1; i <= w; i++)
            {
                // Top.
                SafePlot(grid, size, x - i, y, 0x11);
                SafePlot(grid, size, x - i - 1, y + 1, 0x10);
            }

            for (int i = 1; i < h; i++)
            {
                // Right.
                SafePlot(grid, size, x, y + i, 0x11);
                SafePlot(grid, size, x - 1, y + i + 1, 0x10);
            }
        }

        // Avoid plotting outside symbol or over finder patterns.
        private static void SafePlot(byte[] grid, int size, int x, int y, byte value)
        {
            if ((x >= 0) && (x < size))
            {
                if ((y >= 0) && (y < size))
                {
                    if (grid[(y * size) + x] == 0)
                        grid[(y * size) + x] = value;
                }
            }
        }
        private static char[] GetGB18030Character(char data)
        {
            byte[] gb18030Bytes;
            char[] gb18030 = new char[1];
            char[] gb18030Characters = new char[2];

            try
            {
                gb18030[0] = data;
                gb18030Bytes = Encoding.GetEncoding("GB18030",
                    new EncoderExceptionFallback(),
                    new DecoderExceptionFallback()).GetBytes(gb18030);
            }

            catch (EncoderFallbackException e)
            {
                throw new InvalidDataException("GB18030 Mode: Invalid byte sequence.", e);
            }

            gb18030Characters[0] = (char)(gb18030Bytes[0] << 8 | gb18030Bytes[1] & 0xff);
            if(gb18030Bytes.Length == 4)
                gb18030Characters[1] = (char)(gb18030Bytes[2] << 8 | gb18030Bytes[3] & 0xff);

            return gb18030Characters;
        }
    }
}
