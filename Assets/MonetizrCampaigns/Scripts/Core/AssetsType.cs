//undefine this to test slow internet
//#define TEST_SLOW_LATENCY

//if we define this - video and survey campaigns will work
//#define USING_WEBVIEW

namespace Monetizr.SDK
{
    /// <summary>
    /// Predefined asset types for easier access
    /// </summary>
    public enum AssetsType
    {
        Unknown,
        BrandLogoSprite, //icon
        BrandBannerSprite, //banner
        BrandRewardLogoSprite, //logo
        BrandRewardBannerSprite, //reward_banner
        SurveyURLString, //survey
        //VideoURLString, //video url
        VideoFilePathString, //video url
        BrandTitleString, //text
        TinyTeaserTexture, //text
        TinyTeaserSprite,
        //Html5ZipURLString,
        Html5PathString,
        TiledBackgroundSprite,
        //CampaignHeaderTextColor,
        //CampaignTextColor,
        //HeaderTextColor,
        //CampaignBackgroundColor,
        CustomCoinSprite,
        CustomCoinString,
        LoadingScreenSprite,
        TeaserGifPathString,
        RewardSprite,
        IngameRewardSprite,
        UnknownRewardSprite,

        MinigameSprite1,
        MinigameSprite2,
        MinigameSprite3,
        LeaderboardBannerSprite,

    }

}