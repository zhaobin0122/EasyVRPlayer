namespace NibiruTask
{
    public class CSensorEvent
    {
        public static int TYPE_ACCELEROMETER = 0;
        public static int TYPE_GYROSCOPE = 1;
        public static int TYPE_MAGNETIC = 2;
        int type;
        int deviceId;
        long eventTime;
        float[] values;
        public static int AXIS_X = 0;
        public static int AXIS_Y = 1;
        public static int AXIS_Z = 2;

        public CSensorEvent(int type, int deviceId, long eventTime, float[] values)
        {
            this.type = type;
            this.deviceId = deviceId;
            this.eventTime = eventTime;
            this.values = values;
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

        public float[] getValues()
        {
            return this.values;
        }

        public void setValues(float[] values)
        {
            this.values = values;
        }

        public float getAxis(int axis)
        {
            switch (axis)
            {
                case 0:
                    return this.values[0];
                case 1:
                    return this.values[1];
                case 2:
                    return this.values[2];
                default:
                    return -1.0F;
            }
        }

        public int getType()
        {
            return this.type;
        }

        public void setType(int type)
        {
            this.type = type;
        }

        
        public string toString()
        {
            return "CSensorEvent [type=" + this.type + ", deviceId=" + this.deviceId + ", eventTime=" + this.eventTime + "]";
        }
    }
}