using System;

namespace Skyline.Model{

    public class FileComponent {
        String fileName;
        String contentType;
        byte[] fileBytes;
        int activeIndex;

        public String getFileName() {
            return this.fileName;
        }

        public void setFileName(String fileName) {
            this.fileName = fileName;
        }

        public String getContentType() {
            return this.contentType;
        }

        public void setContentType(String contentType) {
            this.contentType = contentType;
        }

        public byte[] getFileBytes() {
            return this.fileBytes;
        }

        public void setFileBytes(byte[] fileBytes) {
            this.fileBytes = fileBytes;
        }

        public int getActiveIndex() {
            return this.activeIndex;
        }

        public void setActiveIndex(int activeIndex) {
            this.activeIndex = activeIndex;
        }
    }
}