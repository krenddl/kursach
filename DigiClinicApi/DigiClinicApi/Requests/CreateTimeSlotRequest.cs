namespace DigiClinicApi.Requests
{
    public class CreateTimeSlotRequest
    {
        public int DoctorProfileId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
