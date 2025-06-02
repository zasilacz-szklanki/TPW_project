using System.Text.Json;
using System.Text.Json.Serialization;

namespace TP.ConcurrentProgramming.Data
{
    public class BallState
    {
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("ball_id")]
        public int BallId { get; set; }

        [JsonPropertyName("position")]
        public PositionData Position { get; set; }

        [JsonPropertyName("velocity")]
        public VelocityData Velocity { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        public BallState(DateTime timestamp, int ballId, IVector position, IVector velocity, string message)
        {
            Timestamp = timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
            BallId = ballId;
            Position = new PositionData { X = position.x, Y = position.y };
            Velocity = new VelocityData { X = velocity.x, Y = velocity.y };
            Message = message;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public class PositionData
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public class VelocityData
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }
} 