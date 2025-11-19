public abstract class DataBase
{
    public readonly int id = 0;

    public DataBase(SmartXmlNode e)
    {
        this.id = e.GetAttributeInt("ID");
    }

    public virtual void Verify(DataManager manager)
    {

    }
}