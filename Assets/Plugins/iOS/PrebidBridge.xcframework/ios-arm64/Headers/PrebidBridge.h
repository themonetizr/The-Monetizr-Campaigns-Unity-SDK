#import <Foundation/Foundation.h>

#ifdef __cplusplus
extern "C" {
#endif

// Initialization
void prebid_init_default_host(void);
void prebid_init_with_host(const char* hostUrlUtf8);

// Demand
typedef void (*PBUnityCallback)(const char* resultUtf8, void* userData);
void prebid_fetch_demand(const char* prebidDataUtf8,
                         const char* hostUrlUtf8,
                         PBUnityCallback cb,
                         void* userData);

// Privacy helpers (return malloc'ed UTF-8 strings, must free via prebid_free_string)
const char* prebid_get_iab_tcf_consent(void);
const char* prebid_get_iab_us_privacy(void);

// Free any malloc'ed string returned by the bridge
void prebid_free_string(const char* p);

#ifdef __cplusplus
}
#endif
