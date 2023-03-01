
public class ObjectComponent {
    String activeField;
    Object instance;

    public String getActiveField() {
        return activeField;
    }

    public void setActiveField(String activeField) {
        this.activeField = activeField;
    }

    public Object getInstance() {
        return instance;
    }

    public void setInstance(Object instance) {
        this.instance = instance;
    }

    public String toString(){
        return this.activeField + ":" + this.instance;
    }
}
