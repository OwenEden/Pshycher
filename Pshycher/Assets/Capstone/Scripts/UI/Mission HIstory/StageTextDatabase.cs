public static class StageTextDatabase
{
    public static string GetText(int stageId)
    {
        switch (stageId)
        {
            case 1:
                return @"Nantte kakeba ideshouka";

            default:
                return "null";
        }
    }
}