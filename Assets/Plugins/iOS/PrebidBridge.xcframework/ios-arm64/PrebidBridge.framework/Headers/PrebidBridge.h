#import <Foundation/Foundation.h>

#ifdef __cplusplus
extern "C" {
#endif

// Init
void prebid_init_default_host(void);
void prebid_init_with_host(const char* hostUrlUtf8);

// Callback signature back to Unity
typedef void (*PBUnityStaticCallback)(const char* resultUtf8, void* userData);

// Demand
void prebid_fetch_demand(const char* prebidDataUtf8,
                         const char* hostUrlUtf8,
                         PBUnityStaticCallback cb,
                         void* userData);

// Privacy helpers (malloc'ed UTF-8; free with prebid_free_string)
const char* prebid_get_iab_tcf_consent(void);
const char* prebid_get_iab_us_privacy(void);
void prebid_free_string(const char* p);

#ifdef __cplusplus
}
#endif
