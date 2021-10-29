namespace MobileApp.Common.Models.Enums {
    public abstract class AbstractEnum<T> {

        public T Value { get; private set; }

        public AbstractEnum(T value) {
            Value = value;
        }
    }
}
