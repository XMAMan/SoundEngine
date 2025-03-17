namespace MusicMachine.Views.SequenzerVM
{
    //Berechnet die Pixel-Höhe/Y-Position einer Taste abhängig vom toneIndex
    static class PianoViewHelper
    {
        private static bool[] isBlackKey = new bool[] { false, true, false, true, false, false, true, false, true, false, true, false };
        private static int[] posKey = new int[] { 0, 1, 2, 3, 4, 6, 7, 8, 9, 10, 11, 12 };
        private static string[] keyNames = new string[] { "C", "C'", "D", "D'", "E", "F", "F'", "G", "G'", "A", "A'", "H" };
        private static int[] keyNumber = new int[] { 0, 0, 1, 1, 2, 3, 2, 4, 3, 5, 4, 6 }; //Die wie vielte weiße/schwarze Taste ist das in der Oktave?
        
        //Die wie vielte weiße/schwarze Taste ist das in der Oktave?
        public static int GetKeyNumber(int toneIndex)
        {
            return keyNumber[toneIndex % posKey.Length];
        }
        public static string KeyName(int toneIndex)
        {
            string key = keyNames[toneIndex % posKey.Length]; //Position innerhalb der Oktave
            int octave = toneIndex / 12; //Oktavennummer
            if (key.Length == 1) return key + octave;
            return key[0] + octave + "'";
        }

        //Eine Taste ist zwei Striche lange. Wenn seine Linke Kante auf Strichindex 0 liegt, dann liegt seine rechte Kante auf Strichindex 3
        public static int StrichIndex(int toneIndex)
        {
            int pos = posKey[toneIndex % posKey.Length]; //Position innerhalb der Oktave
            int octave = toneIndex / 12; //Oktavennummer
            return octave * 14 + pos;
        }

        public static bool IsBlackKey(int toneIndex)
        {
            return isBlackKey[toneIndex % isBlackKey.Length];
        }

        public static double KeyHeight(double canvasHeight, int minToneIndex, int maxToneIndex)
        {
            return canvasHeight / (PianoViewHelper.StrichIndex(maxToneIndex) - PianoViewHelper.StrichIndex(minToneIndex));
        }

        public static int GetToneIndexFromPixelPos(double canvasHeight, int minToneIndex, int maxToneIndex, double posY)
        {
            double height = PianoViewHelper.KeyHeight(canvasHeight, minToneIndex, maxToneIndex);
            for (int index = minToneIndex; index < maxToneIndex; index++)
            {
                double upperEdge = (canvasHeight - height * 2) - (PianoViewHelper.StrichIndex(index) - PianoViewHelper.StrichIndex(minToneIndex)) * height + height / 2;
                if (posY > upperEdge && posY < upperEdge + height) return index;
            }
            return -1; //Da wo zwei weiße Tasten sind und keine schwarze dazwischen, da gibt es Lücken
        }
    }
}
