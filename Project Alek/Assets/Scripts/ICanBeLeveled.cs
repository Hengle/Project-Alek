using System;

public interface ICanBeLeveled
{
        int CurrentExperience { get; set; }
        
        int ExperienceToNextLevel { get; }

        void AdvanceTowardsNextLevel(int xp);

        int GetNextExperienceThreshold();
        
        Action<int> LevelUpEvent { get; set; }

        void LevelUp();
}