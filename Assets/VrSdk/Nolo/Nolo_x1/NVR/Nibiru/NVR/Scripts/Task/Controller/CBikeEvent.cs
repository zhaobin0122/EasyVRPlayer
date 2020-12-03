namespace NibiruTask
{
    public class CBikeEvent
    {
        private int deviceId;
        private long eventTime;
        private int angle;

        public CBikeEvent(int deviceId, int angle, long eventTime)
        {
            this.deviceId = deviceId;
            this.angle = angle;
            this.eventTime = eventTime;
        }


        public int getDeviceId()
        {
            return this.deviceId;
        }

        public void setDeviceId(int deviceId)
        {
            this.deviceId = deviceId;
        }

        public long getEventTime()
        {
            return this.eventTime;
        }

        public void setEventTime(long eventTime)
        {
            this.eventTime = eventTime;
        }

        public int getAngle()
        {
            return this.angle;
        }

        public void setAngle(int angle)
        {
            this.angle = angle;
        }

        public string toString()
        {
            return "CBikeEvent [deviceId=" + this.deviceId + ", eventTime=" + this.eventTime + ", angle=" + this.angle + "]";
        }
    }
}