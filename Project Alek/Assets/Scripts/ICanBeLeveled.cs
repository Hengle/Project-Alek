using System;

public interface ICanBeLeveled
{
        int CurrentExperience { get; set; }
        
        int ExperienceToNextLevel { get; set; }

        void AdvanceTowardsNextLevel(int xp);

        int GetNextExperienceThreshold(int prev);
        
        Action LevelUpEvent { get; set; }

        void LevelUp();
}