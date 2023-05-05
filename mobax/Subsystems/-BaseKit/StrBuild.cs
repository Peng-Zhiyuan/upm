public class StrBuild {
    static private StrBuild mInstance;

    static public StrBuild Instance {
        get {
            if (mInstance == null) {
                mInstance = new StrBuild();
            }

            return mInstance;
        }
    }

    public System.Text.StringBuilder sb = new System.Text.StringBuilder();
    private int i;
    private char[] int_parser = new char[12];

    public void ClearSB() {
        sb.Length = 0;
        for (i = 0; i < sb.Capacity; i++) {
            sb.Append(' ');
        }

        sb.Length = 0;
    }

    public System.String GetString() {
        return sb.ToString();
    }

    private int argsLength;

    public void Append(params string[] args) {
        argsLength = args.Length;
        for (i = 0; i < argsLength; i++)
            sb.Append(args[i]);
    }

    public void Append(string value) {
        sb.Append(value);
    }

    public string ToStringAppend(params string[] args) {
        ClearSB();
        argsLength = args.Length;
        for (i = 0; i < argsLength; i++)
            sb.Append(args[i]);
        return sb.ToString();
    }

    int count;

    public void Append(int value) {
        if (value >= 0) {
            count = ToCharArray((uint)value, int_parser, 0);
        }
        else {
            int_parser[0] = '-';
            count = ToCharArray((uint)-value, int_parser, 1) + 1;
        }

        for (i = 0; i < count; i++) {
            sb.Append(int_parser[i]);
        }
    }

    public void Append(float value, int accuracy) {
        for (i = 0; i < accuracy; i++) {
            value *= 10;
        }

        value = (int)value;
        if (value >= 0) {
            count = ToCharArray((uint)value, int_parser, 0);
        }
        else {
            int_parser[0] = '-';
            count = ToCharArray((uint)-value, int_parser, 1) + 1;
        }

        for (i = 0; i < count; i++) {
            sb.Append(int_parser[i]);
            if (i == count - accuracy - 1)
                sb.Append(".");
        }
    }

    private static int ToCharArray(uint value, char[] buffer, int bufferIndex) {
        if (value == 0) {
            buffer[bufferIndex] = '0';
            return 1;
        }

        int len = 1;
        for (uint rem = value / 10; rem > 0; rem /= 10) {
            len++;
        }

        for (int i = len - 1; i >= 0; i--) {
            buffer[bufferIndex + i] = (char)('0' + (value % 10));
            value /= 10;
        }

        return len;
    }
}