namespace CauldronCodebase
{
    //this is an abstract class instead of an interface
    //because we need to find assets of this type with a generic method
    public abstract class ConditionalEvent:NightEvent
    {
        public abstract bool Valid(GameDataHandler game);
    }
}