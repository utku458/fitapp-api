namespace FitApp.Models
{
    public class WorkoutProgramExercise
    {
        public int Id { get; set; }
        public int WorkoutProgramId { get; set; }
        public int ExerciseId { get; set; }
        public int OrderIndex { get; set; }
        
        // Navigation properties
        public WorkoutProgram WorkoutProgram { get; set; }
        public Exercise Exercise { get; set; }
    }
}
