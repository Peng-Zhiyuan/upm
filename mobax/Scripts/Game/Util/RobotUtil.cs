public static class RobotUtil
{
    public static bool IsRobot(string uid)
    {
        return uid.StartsWith("z");
    }
}