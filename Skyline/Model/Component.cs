using System;

namespace Skyline.Model{

    public class Component {
        public Component(String component){
            this.component = component;
        }
        String component;
        int activeBeginIndex;
        int activeCloseIndex;

        public String getComponent() {
            return this.component;
        }

        public void setComponent(String component) {
            this.component = component;
        }

        public int getActiveBeginIndex() {
            return this.activeBeginIndex;
        }

        public void setActiveBeginIndex(int activeBeginIndex) {
            this.activeBeginIndex = activeBeginIndex;
        }

        public int getActiveCloseIndex() {
            return this.activeCloseIndex;
        }

        public void setActiveCloseIndex(int activeCloseIndex) {
            this.activeCloseIndex = activeCloseIndex;
        }

    }
}