namespace Upgrades
{
	public class GainedBloodUpgrade : Upgrade<GainedBloodUpgrade>
	{
		private const float BloodPerLevel = 0.2f;
		public        float BloodGained => _level * BloodPerLevel;
		
	}
}