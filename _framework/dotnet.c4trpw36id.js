//! Licensed to the .NET Foundation under one or more agreements.
//! The .NET Foundation licenses this file to you under the MIT license.

const e=()=>(async()=>{try{return new WebAssembly.Module(Uint8Array.from(atob("AGFzbQEAAAABBAFgAAADAgEAChABDgACaR9AAQMAAAsACxoL"),e=>e.codePointAt(0))),!0}catch(e){return!1}})(),o=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,5,1,96,0,1,123,3,2,1,0,10,15,1,13,0,65,1,253,15,65,2,253,15,253,128,2,11])),t=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,5,1,96,0,1,123,3,2,1,0,10,10,1,8,0,65,0,253,15,253,98,11])),n=Symbol.for("wasm promise_control");function r(e,o){let t=null;const r=new Promise(function(n,r){t={isDone:!1,promise:null,resolve:o=>{t.isDone||(t.isDone=!0,n(o),e&&e())},reject:e=>{t.isDone||(t.isDone=!0,r(e),o&&o())}}});t.promise=r;const s=r;return s[n]=t,{promise:s,promise_control:t}}function s(e){return e[n]}function i(e){e&&function(e){return void 0!==e[n]}(e)||We(!1,"Promise is not controllable")}const a="__mono_message__",l=["debug","log","trace","warn","info","error"],c="MONO_WASM: ";let d,u,f,m,g,p;function h(e){m=e}function b(e){if(ke.diagnosticTracing){const o="function"==typeof e?e():e;console.debug(c+o)}}function w(e,...o){console.info(c+e,...o)}function y(e,...o){console.info(e,...o)}function v(e,...o){console.warn(c+e,...o)}function _(e,...o){if(o&&o.length>0&&o[0]&&"object"==typeof o[0]){if(o[0].silent)return;if(o[0].toString)return void console.error(c+e,o[0].toString())}console.error(c+e,...o)}function A(e,o,t){return function(...n){try{let r=n[0];if(void 0===r)r="undefined";else if(null===r)r="null";else if("function"==typeof r)r=r.toString();else if("string"!=typeof r)try{r=JSON.stringify(r)}catch(e){r=r.toString()}o(t?JSON.stringify({method:e,payload:r,arguments:n.slice(1)}):[e+r,...n.slice(1)])}catch(e){f.error(`proxyConsole failed: ${e}`)}}}function x(e,o,t){u=o,m=e,f={...o};const n=`${t}/console`.replace("https://","wss://").replace("http://","ws://");d=new WebSocket(n),d.addEventListener("error",E),d.addEventListener("close",j),function(){for(const e of l)u[e]=A(`console.${e}`,R,!0)}()}function T(e){let o=30;const t=()=>{d?0==d.bufferedAmount||0==o?(e&&y(e),function(){for(const e of l)u[e]=A(`console.${e}`,f.log,!1)}(),d.removeEventListener("error",E),d.removeEventListener("close",j),d.close(1e3,e),d=void 0):(o--,globalThis.setTimeout(t,100)):e&&f&&f.log(e)};t()}function R(e){d&&d.readyState===WebSocket.OPEN?d.send(e):f.log(e)}function E(e){f.error(`[${m}] proxy console websocket error: ${e}`,e)}function j(e){f.debug(`[${m}] proxy console websocket closed: ${e}`,e)}function D(){ke.preferredIcuAsset=C(ke.config);let e="invariant"==ke.config.globalizationMode;if(!e)if(ke.preferredIcuAsset)ke.diagnosticTracing&&b("ICU data archive(s) available, disabling invariant mode");else{if("custom"===ke.config.globalizationMode||"all"===ke.config.globalizationMode||"sharded"===ke.config.globalizationMode){const e="invariant globalization mode is inactive and no ICU data archives are available";throw _(`ERROR: ${e}`),new Error(e)}ke.diagnosticTracing&&b("ICU data archive(s) not available, using invariant globalization mode"),e=!0,ke.preferredIcuAsset=null}const o="DOTNET_SYSTEM_GLOBALIZATION_INVARIANT",t=ke.config.environmentVariables;if(void 0===t[o]&&e&&(t[o]="1"),void 0===t.TZ)try{const e=Intl.DateTimeFormat().resolvedOptions().timeZone||null;e&&(t.TZ=e)}catch(e){w("failed to detect timezone, will fallback to UTC")}}function C(e){var o;if((null===(o=e.resources)||void 0===o?void 0:o.icu)&&"invariant"!=e.globalizationMode){const o=e.applicationCulture||(Me?globalThis.navigator&&globalThis.navigator.languages&&globalThis.navigator.languages[0]:Intl.DateTimeFormat().resolvedOptions().locale);e.applicationCulture||(e.applicationCulture=o);const t=e.resources.icu;let n=null;if("custom"===e.globalizationMode){if(t.length>=1)return t[0].name}else o&&"all"!==e.globalizationMode?"sharded"===e.globalizationMode&&(n=function(e){const o=e.split("-")[0];return"en"===o||["fr","fr-FR","it","it-IT","de","de-DE","es","es-ES"].includes(e)?"icudt_EFIGS.dat":["zh","ko","ja"].includes(o)?"icudt_CJK.dat":"icudt_no_CJK.dat"}(o)):n="icudt.dat";if(n)for(let e=0;e<t.length;e++){const o=t[e];if(o.virtualPath===n)return o.name}}return e.globalizationMode="invariant",null}(new Date).valueOf();const M=class{constructor(e){this.url=e}toString(){return this.url}};async function S(e){if(Se&&"function"!=typeof globalThis.atob){const e="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";globalThis.atob=o=>{const t=String(o).replace(/=+$/,"");let n="";for(let o=0,r=0,s=0;s<t.length;s++){const i=e.indexOf(t.charAt(s));-1!==i&&(r=o%4?64*r+i:i,o++%4&&(n+=String.fromCharCode(255&r>>(-2*o&6))))}return n}}if(Ee){const e=await import(/*! webpackIgnore: true */"process"),o=14;if(e.versions.node.split(".")[0]<o)throw new Error(`NodeJS at '${e.execPath}' has too low version '${e.versions.node}', please use at least ${o}.`)}const o=/*! webpackIgnore: true */import.meta.url,t=o.indexOf("?");var n;if(t>0&&(ke.modulesUniqueQuery=o.substring(t)),ke.scriptUrl=o.replace(/\\/g,"/").replace(/[?#].*/,""),ke.scriptDirectory=(n=ke.scriptUrl).slice(0,n.lastIndexOf("/"))+"/",ke.locateFile=e=>"URL"in globalThis&&globalThis.URL!==M?new URL(e,ke.scriptDirectory).toString():I(e)?e:ke.scriptDirectory+e,ke.fetch_like=U,ke.out=console.log,ke.err=console.error,ke.onDownloadResourceProgress=e.onDownloadResourceProgress,Me&&globalThis.navigator){const e=globalThis.navigator,o=e.userAgentData&&e.userAgentData.brands;o&&o.length>0?ke.isChromium=o.some(e=>"Google Chrome"===e.brand||"Microsoft Edge"===e.brand||"Chromium"===e.brand):e.userAgent&&(ke.isChromium=e.userAgent.includes("Chrome"),ke.isFirefox=e.userAgent.includes("Firefox"))}void 0===globalThis.URL&&(globalThis.URL=M)}async function U(e,o){try{const t="function"==typeof globalThis.fetch;if(Ee){const n=e.startsWith("file://");if(!n&&t)return globalThis.fetch(e,o||{credentials:"same-origin"});g||(p=await import(/*! webpackIgnore: true */"url"),g=await import(/*! webpackIgnore: true */"fs")),n&&(e=p.fileURLToPath(e));const r=await g.promises.readFile(e);return{ok:!0,headers:{length:0,get:()=>null},url:e,arrayBuffer:()=>r,json:()=>JSON.parse(r),text:()=>{throw new Error("NotImplementedException")}}}if(t)return globalThis.fetch(e,o||{credentials:"same-origin"});if("function"==typeof read)return{ok:!0,url:e,headers:{length:0,get:()=>null},arrayBuffer:()=>new Uint8Array(read(e,"binary")),json:()=>JSON.parse(read(e,"utf8")),text:()=>read(e,"utf8")}}catch(o){return{ok:!1,url:e,status:500,headers:{length:0,get:()=>null},statusText:"ERR28: "+o,arrayBuffer:()=>{throw o},json:()=>{throw o},text:()=>{throw o}}}throw new Error("No fetch implementation available")}const P=/^[a-zA-Z][a-zA-Z\d+\-.]*?:\/\//,k=/[a-zA-Z]:[\\/]/;function I(e){return Ee||Se?e.startsWith("/")||e.startsWith("\\")||-1!==e.indexOf("///")||k.test(e):P.test(e)}let O,$=0;const L=[],N=[],z=new Map,F={"js-module-runtime":!0,"js-module-dotnet":!0,"js-module-native":!0,"js-module-diagnostics":!0},W={...F,"js-module-library-initializer":!0},V={...F,dotnetwasm:!0,heap:!0,manifest:!0},B={...W,manifest:!0},J={...W,dotnetwasm:!0},H={dotnetwasm:!0,symbols:!0},Q={...W,dotnetwasm:!0,symbols:!0},q={symbols:!0};function G(e){return!("icu"==e.behavior&&e.name!=ke.preferredIcuAsset)}function Z(e,o,t){null!=o||(o=[]),We(1==o.length,`Expect to have one ${t} asset in resources`);const n=o[0];return n.behavior=t,K(n),e.push(n),n}function K(e){V[e.behavior]&&z.set(e.behavior,e)}function X(e){We(V[e],`Unknown single asset behavior ${e}`);const o=z.get(e);if(o&&!o.resolvedUrl)if(o.resolvedUrl=ke.locateFile(o.name),F[o.behavior]){const e=me(o);e?("string"!=typeof e&&We(!1,"loadBootResource response for 'dotnetjs' type should be a URL string"),o.resolvedUrl=e):o.resolvedUrl=le(o.resolvedUrl,o.behavior)}else if("dotnetwasm"!==o.behavior)throw new Error(`Unknown single asset behavior ${e}`);return o}function Y(e){const o=X(e);return We(o,`Single asset for ${e} not found`),o}let ee=!1;async function oe(){if(!ee){ee=!0,ke.diagnosticTracing&&b("mono_download_assets");try{const e=[],o=[],t=(e,o)=>{!Q[e.behavior]&&G(e)&&ke.expected_instantiated_assets_count++,!J[e.behavior]&&G(e)&&(ke.expected_downloaded_assets_count++,o.push(se(e)))};for(const o of L)t(o,e);for(const e of N)t(e,o);ke.allDownloadsQueued.promise_control.resolve(),Promise.all([...e,...o]).then(()=>{ke.allDownloadsFinished.promise_control.resolve()}).catch(e=>{throw ke.err("Error in mono_download_assets: "+e),Xe(1,e),e}),await ke.runtimeModuleLoaded.promise;const n=async e=>{const o=await e;if(H[o.behavior])return"symbols"===o.behavior&&(await Pe.instantiate_symbols_asset(o),ge(o)),void++ke.actual_downloaded_assets_count;if(o.buffer){if(!Q[o.behavior]){o.buffer&&"object"==typeof o.buffer||We(!1,"asset buffer must be array-like or buffer-like or promise of these"),"string"!=typeof o.resolvedUrl&&We(!1,"resolvedUrl must be string");const e=o.resolvedUrl,t=await o.buffer,n=new Uint8Array(t);ge(o),await Pe.beforeOnRuntimeInitialized.promise,await Pe.afterInstantiateWasm.promise,Pe.instantiate_asset(o,e,n)}}else o.isOptional||We(!1,"Expected asset to have the downloaded buffer"),!J[o.behavior]&&G(o)&&ke.expected_downloaded_assets_count--,!Q[o.behavior]&&G(o)&&ke.expected_instantiated_assets_count--},r=[],s=[];for(const o of e)r.push(n(o));for(const e of o)s.push(n(e));Promise.all(r).then(()=>{Ce||Pe.coreAssetsInMemory.promise_control.resolve()}).catch(e=>{throw ke.err("Error in mono_download_assets: "+e),Xe(1,e),e}),Promise.all(s).then(async()=>{Ce||(await Pe.coreAssetsInMemory.promise,Pe.allAssetsInMemory.promise_control.resolve())}).catch(e=>{throw ke.err("Error in mono_download_assets: "+e),Xe(1,e),e})}catch(e){throw ke.err("Error in mono_download_assets: "+e),e}}}let te=!1;function ne(){if(te)return;te=!0;const e=ke.config,o=[];if(e.assets)for(const o of e.assets)"object"!=typeof o&&We(!1,`asset must be object, it was ${typeof o} : ${o}`),"string"!=typeof o.behavior&&We(!1,"asset behavior must be known string"),"string"!=typeof o.name&&We(!1,"asset name must be string"),o.resolvedUrl&&"string"!=typeof o.resolvedUrl&&We(!1,"asset resolvedUrl could be string"),o.hash&&"string"!=typeof o.hash&&We(!1,"asset resolvedUrl could be string"),o.pendingDownload&&"object"!=typeof o.pendingDownload&&We(!1,"asset pendingDownload could be object"),o.isCore?L.push(o):N.push(o),K(o);else if(e.resources){const t=e.resources;t.wasmNative||We(!1,"resources.wasmNative must be defined"),t.jsModuleNative||We(!1,"resources.jsModuleNative must be defined"),t.jsModuleRuntime||We(!1,"resources.jsModuleRuntime must be defined"),Z(N,t.wasmNative,"dotnetwasm"),Z(o,t.jsModuleNative,"js-module-native"),Z(o,t.jsModuleRuntime,"js-module-runtime"),t.jsModuleDiagnostics&&Z(o,t.jsModuleDiagnostics,"js-module-diagnostics");const n=(e,o,t)=>{const n=e;n.behavior=o,t?(n.isCore=!0,L.push(n)):N.push(n)};if(t.coreAssembly)for(let e=0;e<t.coreAssembly.length;e++)n(t.coreAssembly[e],"assembly",!0);if(t.assembly)for(let e=0;e<t.assembly.length;e++)n(t.assembly[e],"assembly",!t.coreAssembly);if(0!=e.debugLevel&&ke.isDebuggingSupported()){if(t.corePdb)for(let e=0;e<t.corePdb.length;e++)n(t.corePdb[e],"pdb",!0);if(t.pdb)for(let e=0;e<t.pdb.length;e++)n(t.pdb[e],"pdb",!t.corePdb)}if(e.loadAllSatelliteResources&&t.satelliteResources)for(const e in t.satelliteResources)for(let o=0;o<t.satelliteResources[e].length;o++){const r=t.satelliteResources[e][o];r.culture=e,n(r,"resource",!t.coreAssembly)}if(t.coreVfs)for(let e=0;e<t.coreVfs.length;e++)n(t.coreVfs[e],"vfs",!0);if(t.vfs)for(let e=0;e<t.vfs.length;e++)n(t.vfs[e],"vfs",!t.coreVfs);const r=C(e);if(r&&t.icu)for(let e=0;e<t.icu.length;e++){const o=t.icu[e];o.name===r&&n(o,"icu",!1)}if(t.wasmSymbols)for(let e=0;e<t.wasmSymbols.length;e++)n(t.wasmSymbols[e],"symbols",!1)}if(e.appsettings)for(let o=0;o<e.appsettings.length;o++){const t=e.appsettings[o],n=pe(t);"appsettings.json"!==n&&n!==`appsettings.${e.applicationEnvironment}.json`||N.push({name:t,behavior:"vfs",cache:"no-cache",useCredentials:!0})}e.assets=[...L,...N,...o]}async function re(e){const o=await se(e);return await o.pendingDownloadInternal.response,o.buffer}async function se(e){try{return await ie(e)}catch(o){if(!ke.enableDownloadRetry)throw o;if(Se||Ee)throw o;if(e.pendingDownload&&e.pendingDownloadInternal==e.pendingDownload)throw o;if(e.resolvedUrl&&-1!=e.resolvedUrl.indexOf("file://"))throw o;if(o&&404==o.status)throw o;e.pendingDownloadInternal=void 0,await ke.allDownloadsQueued.promise;try{return ke.diagnosticTracing&&b(`Retrying download '${e.name}'`),await ie(e)}catch(o){return e.pendingDownloadInternal=void 0,await new Promise(e=>globalThis.setTimeout(e,100)),ke.diagnosticTracing&&b(`Retrying download (2) '${e.name}' after delay`),await ie(e)}}}async function ie(e){for(;O;)await O.promise;try{++$,$==ke.maxParallelDownloads&&(ke.diagnosticTracing&&b("Throttling further parallel downloads"),O=r());const o=await async function(e){if(e.pendingDownload&&(e.pendingDownloadInternal=e.pendingDownload),e.pendingDownloadInternal&&e.pendingDownloadInternal.response)return e.pendingDownloadInternal.response;if(e.buffer){const o=await e.buffer;return e.resolvedUrl||(e.resolvedUrl="undefined://"+e.name),e.pendingDownloadInternal={url:e.resolvedUrl,name:e.name,response:Promise.resolve({ok:!0,arrayBuffer:()=>o,json:()=>JSON.parse(new TextDecoder("utf-8").decode(o)),text:()=>new TextDecoder("utf-8").decode(o),headers:{get:()=>{}}})},e.pendingDownloadInternal.response}const o=e.loadRemote&&ke.config.remoteSources?ke.config.remoteSources:[""];let t;for(let n of o){n=n.trim(),"./"===n&&(n="");const o=ae(e,n);e.name===o?ke.diagnosticTracing&&b(`Attempting to download '${o}'`):ke.diagnosticTracing&&b(`Attempting to download '${o}' for ${e.name}`);try{e.resolvedUrl=o;const n=ue(e);if(e.pendingDownloadInternal=n,t=await n.response,!t||!t.ok)continue;return t}catch(e){t||(t={ok:!1,url:o,status:0,statusText:""+e});continue}}const n=e.isOptional||e.name.match(/\.pdb$/)&&ke.config.ignorePdbLoadErrors;if(t||We(!1,`Response undefined ${e.name}`),!n){const o=new Error(`download '${t.url}' for ${e.name} failed ${t.status} ${t.statusText}`);throw o.status=t.status,o}w(`optional download '${t.url}' for ${e.name} failed ${t.status} ${t.statusText}`)}(e);return o?(H[e.behavior]||(e.buffer=await o.arrayBuffer(),++ke.actual_downloaded_assets_count),e):e}finally{if(--$,O&&$==ke.maxParallelDownloads-1){ke.diagnosticTracing&&b("Resuming more parallel downloads");const e=O;O=void 0,e.promise_control.resolve()}}}function ae(e,o){let t;return null==o&&We(!1,`sourcePrefix must be provided for ${e.name}`),e.resolvedUrl?t=e.resolvedUrl:(t=""===o?"assembly"===e.behavior||"pdb"===e.behavior?e.name:"resource"===e.behavior&&e.culture&&""!==e.culture?`${e.culture}/${e.name}`:e.name:o+e.name,t=le(ke.locateFile(t),e.behavior)),t&&"string"==typeof t||We(!1,"attemptUrl need to be path or url string"),t}function le(e,o){return ke.modulesUniqueQuery&&B[o]&&(e+=ke.modulesUniqueQuery),e}let ce=0;const de=new Set;function ue(e){try{e.resolvedUrl||We(!1,"Request's resolvedUrl must be set");const o=function(e){let o=e.resolvedUrl;if(ke.loadBootResource){const t=me(e);if(t instanceof Promise)return t;"string"==typeof t&&(o=t)}const t={};return e.cache?t.cache=e.cache:ke.config.disableNoCacheFetch||(t.cache="no-cache"),e.useCredentials?t.credentials="include":!ke.config.disableIntegrityCheck&&e.hash&&(t.integrity=e.hash),ke.fetch_like(o,t)}(e),t={name:e.name,url:e.resolvedUrl,response:o};return de.add(e.name),t.response.then(()=>{"assembly"==e.behavior&&ke.loadedAssemblies.push(e.name),ce++,ke.onDownloadResourceProgress&&ke.onDownloadResourceProgress(ce,de.size)}),t}catch(o){const t={ok:!1,url:e.resolvedUrl,status:500,statusText:"ERR29: "+o,arrayBuffer:()=>{throw o},json:()=>{throw o}};return{name:e.name,url:e.resolvedUrl,response:Promise.resolve(t)}}}const fe={resource:"assembly",assembly:"assembly",pdb:"pdb",icu:"globalization",vfs:"configuration",manifest:"manifest",dotnetwasm:"dotnetwasm","js-module-dotnet":"dotnetjs","js-module-native":"dotnetjs","js-module-runtime":"dotnetjs"};function me(e){var o;if(ke.loadBootResource){const t=null!==(o=e.hash)&&void 0!==o?o:"",n=e.resolvedUrl,r=fe[e.behavior];if(r){const o=ke.loadBootResource(r,e.name,n,t,e.behavior);return"string"==typeof o?function(e){return"string"!=typeof e&&We(!1,"url must be a string"),!I(e)&&0!==e.indexOf("./")&&0!==e.indexOf("../")&&globalThis.URL&&globalThis.document&&globalThis.document.baseURI&&(e=new URL(e,globalThis.document.baseURI).toString()),e}(o):o}}}function ge(e){e.pendingDownloadInternal=null,e.pendingDownload=null,e.buffer=null,e.moduleExports=null}function pe(e){let o=e.lastIndexOf("/");return o>=0&&o++,e.substring(o)}async function he(e){e&&await Promise.all((null!=e?e:[]).map(e=>async function(e){try{const o=e.name;if(!e.moduleExports){const t=le(ke.locateFile(o),"js-module-library-initializer");ke.diagnosticTracing&&b(`Attempting to import '${t}' for ${e}`),e.moduleExports=await import(/*! webpackIgnore: true */t)}ke.libraryInitializers.push({scriptName:o,exports:e.moduleExports})}catch(o){v(`Failed to import library initializer '${e}': ${o}`)}}(e)))}async function be(e,o){if(!ke.libraryInitializers)return;const t=[];for(let n=0;n<ke.libraryInitializers.length;n++){const r=ke.libraryInitializers[n];r.exports[e]&&t.push(we(r.scriptName,e,()=>r.exports[e](...o)))}await Promise.all(t)}async function we(e,o,t){try{await t()}catch(t){throw v(`Failed to invoke '${o}' on library initializer '${e}': ${t}`),Xe(1,t),t}}function ye(e,o){if(e===o)return e;const t={...o};return void 0!==t.assets&&t.assets!==e.assets&&(t.assets=[...e.assets||[],...t.assets||[]]),void 0!==t.resources&&(t.resources=_e(e.resources||{assembly:[],jsModuleNative:[],jsModuleRuntime:[],wasmNative:[]},t.resources)),void 0!==t.environmentVariables&&(t.environmentVariables={...e.environmentVariables||{},...t.environmentVariables||{}}),void 0!==t.runtimeOptions&&t.runtimeOptions!==e.runtimeOptions&&(t.runtimeOptions=[...e.runtimeOptions||[],...t.runtimeOptions||[]]),Object.assign(e,t)}function ve(e,o){if(e===o)return e;const t={...o};return t.config&&(e.config||(e.config={}),t.config=ye(e.config,t.config)),Object.assign(e,t)}function _e(e,o){if(e===o)return e;const t={...o};return void 0!==t.coreAssembly&&(t.coreAssembly=[...e.coreAssembly||[],...t.coreAssembly||[]]),void 0!==t.assembly&&(t.assembly=[...e.assembly||[],...t.assembly||[]]),void 0!==t.lazyAssembly&&(t.lazyAssembly=[...e.lazyAssembly||[],...t.lazyAssembly||[]]),void 0!==t.corePdb&&(t.corePdb=[...e.corePdb||[],...t.corePdb||[]]),void 0!==t.pdb&&(t.pdb=[...e.pdb||[],...t.pdb||[]]),void 0!==t.jsModuleNative&&(t.jsModuleNative=[...e.jsModuleNative||[],...t.jsModuleNative||[]]),void 0!==t.jsModuleDiagnostics&&(t.jsModuleDiagnostics=[...e.jsModuleDiagnostics||[],...t.jsModuleDiagnostics||[]]),void 0!==t.jsModuleRuntime&&(t.jsModuleRuntime=[...e.jsModuleRuntime||[],...t.jsModuleRuntime||[]]),void 0!==t.wasmSymbols&&(t.wasmSymbols=[...e.wasmSymbols||[],...t.wasmSymbols||[]]),void 0!==t.wasmNative&&(t.wasmNative=[...e.wasmNative||[],...t.wasmNative||[]]),void 0!==t.icu&&(t.icu=[...e.icu||[],...t.icu||[]]),void 0!==t.satelliteResources&&(t.satelliteResources=function(e,o){if(e===o)return e;for(const t in o)e[t]=[...e[t]||[],...o[t]||[]];return e}(e.satelliteResources||{},t.satelliteResources||{})),void 0!==t.modulesAfterConfigLoaded&&(t.modulesAfterConfigLoaded=[...e.modulesAfterConfigLoaded||[],...t.modulesAfterConfigLoaded||[]]),void 0!==t.modulesAfterRuntimeReady&&(t.modulesAfterRuntimeReady=[...e.modulesAfterRuntimeReady||[],...t.modulesAfterRuntimeReady||[]]),void 0!==t.extensions&&(t.extensions={...e.extensions||{},...t.extensions||{}}),void 0!==t.vfs&&(t.vfs=[...e.vfs||[],...t.vfs||[]]),Object.assign(e,t)}function Ae(){const e=ke.config;if(e.environmentVariables=e.environmentVariables||{},e.runtimeOptions=e.runtimeOptions||[],e.resources=e.resources||{assembly:[],jsModuleNative:[],jsModuleRuntime:[],wasmNative:[],vfs:[],satelliteResources:{}},e.assets){ke.diagnosticTracing&&b("config.assets is deprecated, use config.resources instead");for(const o of e.assets){const t={};switch(o.behavior){case"assembly":t.assembly=[o];break;case"pdb":t.pdb=[o];break;case"resource":t.satelliteResources={},t.satelliteResources[o.culture]=[o];break;case"icu":t.icu=[o];break;case"symbols":t.wasmSymbols=[o];break;case"vfs":t.vfs=[o];break;case"dotnetwasm":t.wasmNative=[o];break;case"js-module-runtime":t.jsModuleRuntime=[o];break;case"js-module-native":t.jsModuleNative=[o];break;case"js-module-diagnostics":t.jsModuleDiagnostics=[o];break;case"js-module-dotnet":break;default:throw new Error(`Unexpected behavior ${o.behavior} of asset ${o.name}`)}_e(e.resources,t)}}e.debugLevel,void 0===e.virtualWorkingDirectory&&(e.virtualWorkingDirectory=Ue),e.applicationEnvironment||(e.applicationEnvironment="Production"),e.applicationCulture&&(e.environmentVariables.LANG=`${e.applicationCulture}.UTF-8`),Pe.diagnosticTracing=ke.diagnosticTracing=!!e.diagnosticTracing,Pe.waitForDebugger=e.waitForDebugger,ke.maxParallelDownloads=e.maxParallelDownloads||ke.maxParallelDownloads,ke.enableDownloadRetry=void 0!==e.enableDownloadRetry?e.enableDownloadRetry:ke.enableDownloadRetry}let xe=!1;async function Te(e){var o;if(xe)await ke.afterConfigLoaded.promise;else try{if(xe=!0,Ae(),await he(null===(o=ke.config.resources)||void 0===o?void 0:o.modulesAfterConfigLoaded),await be("onRuntimeConfigLoaded",[ke.config]),e.onConfigLoaded)try{await e.onConfigLoaded(ke.config,Oe),Ae()}catch(e){throw _("onConfigLoaded() failed",e),e}Ae(),ke.afterConfigLoaded.promise_control.resolve(ke.config)}catch(o){const t=`Failed to initialize config ${o} ${null==o?void 0:o.stack}`;throw ke.config=e.config=Object.assign(ke.config,{message:t,error:o,isError:!0}),Xe(1,new Error(t)),o}}function Re(){return!!globalThis.navigator&&(ke.isChromium||ke.isFirefox)}"function"==typeof importScripts&&(globalThis.dotnetSidecar=!0);const Ee="object"==typeof process&&"object"==typeof process.versions&&"string"==typeof process.versions.node,je="function"==typeof importScripts,De=je&&"undefined"!=typeof dotnetSidecar,Ce=je&&!De,Me="object"==typeof window||je&&!Ee,Se=!Me&&!Ee,Ue="/";let Pe={},ke={},Ie={},Oe={},$e={},Le=!1;const Ne={},ze={config:Ne},Fe={mono:{},binding:{},internal:$e,module:ze,loaderHelpers:ke,runtimeHelpers:Pe,diagnosticHelpers:Ie,api:Oe};function We(e,o){if(e)return;const t="Assert failed: "+("function"==typeof o?o():o),n=new Error(t);_(t,n),Pe.nativeAbort(n)}function Ve(){return void 0!==ke.exitCode}function Be(){return Pe.runtimeReady&&!Ve()}function Je(){Ve()&&We(!1,`.NET runtime already exited with ${ke.exitCode} ${ke.exitReason}. You can use dotnet.runMain() which doesn't exit the runtime.`),Pe.runtimeReady||We(!1,".NET runtime didn't start yet. Please call dotnet.create() first.")}function He(){Me&&(globalThis.addEventListener("unhandledrejection",eo),globalThis.addEventListener("error",oo))}let Qe,qe;function Ge(){Qe=ze.onAbort,qe=ze.onExit,ze.onAbort=Ke,ze.onExit=Ze}function Ze(e){qe&&qe(e),Xe(e,ke.exitReason)}function Ke(e){Qe&&Qe(e||ke.exitReason),Xe(1,e||ke.exitReason)}function Xe(e,o){var t;const n=o&&"object"==typeof o;e=n&&"number"==typeof o.status?o.status:void 0===e?-1:e;const r=n&&"string"==typeof o.message?o.message:""+o;(o=n?o:Pe.ExitStatus?function(e,o){const t=new Pe.ExitStatus(e);return t.message=o,t.toString=()=>o,t}(e,r):new Error("Exit with code "+e+" "+r)).status=e,o.message||(o.message=r);const s=""+(o.stack||(new Error).stack);try{Object.defineProperty(o,"stack",{get:()=>s})}catch(e){}const i=!!o.silent;if(o.silent=!0,Ve())ke.diagnosticTracing&&b("mono_exit called after exit");else{try{ze.onAbort==Ke&&(ze.onAbort=Qe),ze.onExit==Ze&&(ze.onExit=qe),Me&&(globalThis.removeEventListener("unhandledrejection",eo),globalThis.removeEventListener("error",oo)),Pe.runtimeReady?(Pe.jiterpreter_dump_stats&&Pe.jiterpreter_dump_stats(!1),0===e&&(null===(t=ke.config)||void 0===t?void 0:t.interopCleanupOnExit)&&Pe.forceDisposeProxies(!0,!0)):(ke.diagnosticTracing&&b(`abort_startup, reason: ${o}`),function(e){ke.allDownloadsQueued.promise_control.reject(e),ke.allDownloadsFinished.promise_control.reject(e),ke.afterConfigLoaded.promise_control.reject(e),ke.wasmCompilePromise.promise_control.reject(e),ke.runtimeModuleLoaded.promise_control.reject(e),Pe.dotnetReady&&(Pe.dotnetReady.promise_control.reject(e),Pe.afterInstantiateWasm.promise_control.reject(e),Pe.afterPreRun.promise_control.reject(e),Pe.beforeOnRuntimeInitialized.promise_control.reject(e),Pe.afterOnRuntimeInitialized.promise_control.reject(e),Pe.afterPostRun.promise_control.reject(e))}(o))}catch(e){v("mono_exit A failed",e)}try{i||(function(e,o){if(0!==e&&o){const e=Pe.ExitStatus&&o instanceof Pe.ExitStatus?b:_;"string"==typeof o?e(o):(void 0===o.stack&&(o.stack=(new Error).stack+""),o.message?e(Pe.stringify_as_error_with_stack?Pe.stringify_as_error_with_stack(o.message+"\n"+o.stack):o.message+"\n"+o.stack):e(JSON.stringify(o)))}!Ce&&ke.config&&(ke.config.logExitCode?ke.config.forwardConsole?T("WASM EXIT "+e):y("WASM EXIT "+e):ke.config.forwardConsole&&T())}(e,o),function(e){if(Me&&!Ce&&ke.config&&ke.config.appendElementOnExit&&document){const o=document.createElement("label");o.id="tests_done",0!==e&&(o.style.background="red"),o.innerHTML=""+e,document.body.appendChild(o)}}(e))}catch(e){v("mono_exit B failed",e)}ke.exitCode=e,ke.exitReason||(ke.exitReason=o),!Ce&&Pe.runtimeReady&&ze.runtimeKeepalivePop()}if(ke.config&&ke.config.asyncFlushOnExit&&0===e)throw(async()=>{try{await async function(){if(Ee)try{const e=await import(/*! webpackIgnore: true */"process"),o=e=>new Promise((o,t)=>{e.on("error",t),e.end("","utf8",o)}),t=o(e.stderr),n=o(e.stdout);let r;const s=new Promise(e=>{r=setTimeout(()=>e("timeout"),1e3)});await Promise.race([Promise.all([n,t]),s]),clearTimeout(r)}catch(e){_(`flushing std* streams failed: ${e}`)}}()}finally{Ye(e,o)}})(),o;Ye(e,o)}function Ye(e,o){if(Pe.runtimeReady&&Pe.nativeExit)try{Pe.nativeExit(e)}catch(e){!Pe.ExitStatus||e instanceof Pe.ExitStatus||v("set_exit_code_and_quit_now failed: "+e.toString())}if(0!==e||!Me)throw Ee?process.exit(e):Pe.quit&&Pe.quit(e,o),o}function eo(e){to(e,e.reason,"rejection")}function oo(e){to(e,e.error,"error")}function to(e,o,t){e.preventDefault();try{o||(o=new Error("Unhandled "+t)),void 0===o.stack&&(o.stack=(new Error).stack),o.stack=o.stack+"",o.silent||(_("Unhandled error:",o),Xe(1,o))}catch(e){}}!function(n){if(Le)throw new Error("Loader module already loaded");Le=!0,Pe=n.runtimeHelpers,ke=n.loaderHelpers,Ie=n.diagnosticHelpers,Oe=n.api,$e=n.internal,Object.assign(Oe,{INTERNAL:$e,invokeLibraryInitializers:be}),Object.assign(n.module,{config:ye(Ne,{environmentVariables:{}})});const a={mono_wasm_bindings_is_ready:!1,config:n.module.config,diagnosticTracing:!1,nativeAbort:e=>{throw e||new Error("abort")},nativeExit:e=>{throw new Error("exit:"+e)}},l={gitHash:"297acf930fb0f5c07fd4e51d78e521b4ad2e5d1f",config:n.module.config,diagnosticTracing:!1,maxParallelDownloads:16,enableDownloadRetry:!0,_loaded_files:[],loadedFiles:[],loadedAssemblies:[],libraryInitializers:[],workerNextNumber:1,actual_downloaded_assets_count:0,actual_instantiated_assets_count:0,expected_downloaded_assets_count:0,expected_instantiated_assets_count:0,afterConfigLoaded:r(),allDownloadsQueued:r(),allDownloadsFinished:r(),wasmCompilePromise:r(),runtimeModuleLoaded:r(),loadingWorkers:r(),is_exited:Ve,is_runtime_running:Be,assert_runtime_running:Je,mono_exit:Xe,createPromiseController:r,getPromiseController:s,assertIsControllablePromise:i,mono_download_assets:oe,resolve_single_asset_path:Y,setup_proxy_console:x,set_thread_prefix:h,installUnhandledErrorHandler:He,retrieve_asset_download:re,invokeLibraryInitializers:be,isDebuggingSupported:Re,exceptionsFinal:e,simd:t,relaxedSimd:o};Object.assign(Pe,a),Object.assign(ke,l)}(Fe);let no,ro,so,io=!1,ao=!1;async function lo(e){if(!ao){if(ao=!0,Me&&ke.config.forwardConsole&&void 0!==globalThis.WebSocket&&x("main",globalThis.console,globalThis.location.origin),ze||We(!1,"Null moduleConfig"),ke.config||We(!1,"Null moduleConfig.config"),"function"==typeof e){const o=e(Fe.api);if(o.ready)throw new Error("Module.ready couldn't be redefined.");Object.assign(ze,o),ve(ze,o)}else{if("object"!=typeof e)throw new Error("Can't use moduleFactory callback of createDotnetRuntime function.");ve(ze,e)}await S(ze)}}async function co(e){return await lo(e),ke.config.exitOnUnhandledError&&He(),Ge(),async function(){var e;await Te(ze),ne();const o=uo();(async function(){try{const e=Y("dotnetwasm");await se(e),e&&e.pendingDownloadInternal&&e.pendingDownloadInternal.response||We(!1,"Can't load dotnet.native.wasm");const o=await e.pendingDownloadInternal.response,t=o.headers&&o.headers.get?o.headers.get("Content-Type"):void 0;let n;if("function"==typeof WebAssembly.compileStreaming&&"application/wasm"===t)n=await WebAssembly.compileStreaming(o);else{Me&&"application/wasm"!==t&&v('WebAssembly resource does not have the expected content type "application/wasm", so falling back to slower ArrayBuffer instantiation.');const e=await o.arrayBuffer();ke.diagnosticTracing&&b("instantiate_wasm_module buffered"),n=Se?await Promise.resolve(new WebAssembly.Module(e)):await WebAssembly.compile(e)}e.pendingDownloadInternal=null,e.pendingDownload=null,e.buffer=null,e.moduleExports=null,ke.wasmCompilePromise.promise_control.resolve(n)}catch(e){ke.wasmCompilePromise.promise_control.reject(e)}})(),setTimeout(async()=>{try{D(),await oe()}catch(e){Xe(1,e)}},0);const t=await Promise.all(o);return await fo(t),await Pe.dotnetReady.promise,await he(null===(e=ke.config.resources)||void 0===e?void 0:e.modulesAfterRuntimeReady),await be("onRuntimeReady",[Fe.api]),Oe}()}function uo(){const e=Y("js-module-runtime"),o=Y("js-module-native");if(no&&ro)return[no,ro,so];"object"==typeof e.moduleExports?no=e.moduleExports:(ke.diagnosticTracing&&b(`Attempting to import '${e.resolvedUrl}' for ${e.name}`),no=import(/*! webpackIgnore: true */e.resolvedUrl)),"object"==typeof o.moduleExports?ro=o.moduleExports:(ke.diagnosticTracing&&b(`Attempting to import '${o.resolvedUrl}' for ${o.name}`),ro=import(/*! webpackIgnore: true */o.resolvedUrl));const t=X("js-module-diagnostics");return t&&("object"==typeof t.moduleExports?so=t.moduleExports:(ke.diagnosticTracing&&b(`Attempting to import '${t.resolvedUrl}' for ${t.name}`),so=import(/*! webpackIgnore: true */t.resolvedUrl))),[no,ro,so]}async function fo(e){const{initializeExports:o,initializeReplacements:t,configureRuntimeStartup:n,configureEmscriptenStartup:r,configureWorkerStartup:s,setRuntimeGlobals:i,passEmscriptenInternals:a}=e[0],{default:l}=e[1],c=e[2];i(Fe),o(Fe),c&&c.setRuntimeGlobals(Fe),await n(ze),ke.runtimeModuleLoaded.promise_control.resolve(),l(()=>(Object.assign(ze,{__dotnet_runtime:{initializeReplacements:t,configureEmscriptenStartup:r,configureWorkerStartup:s,passEmscriptenInternals:a}}),ze)).catch(e=>{if(e.message&&e.message.toLowerCase().includes("out of memory"))throw new Error(".NET runtime has failed to start, because too much memory was requested. Please decrease the memory by adjusting EmccMaximumHeapSize.");throw e})}Ce&&async function(){(function(){const e=new MessageChannel,o=e.port1,t=e.port2;o.addEventListener("message",e=>{!function(e){const o=JSON.parse(e.config),t=JSON.parse(e.monoThreadInfo);ze.config=o,ze.wasmModule=e.wasmModule,ze.wasmMemory=e.wasmMemory,ze.handlers=e.handlers,io?ke.diagnosticTracing&&b("mono config already received"):(ye(ke.config,o),Pe.monoThreadInfo=t,Ae(),ke.diagnosticTracing&&b("mono config received"),io=!0,ke.afterConfigLoaded.promise_control.resolve(ke.config),Me&&o.forwardConsole&&void 0!==globalThis.WebSocket&&ke.setup_proxy_console("worker-idle",console,globalThis.location.origin))}(e.data),o.close(),t.close()},{once:!0}),o.start(),self.postMessage({[a]:{monoCmd:"preload",port:t}},[t])})(),await ke.afterConfigLoaded.promise,function(){const e=ke.config;e.assets||We(!1,"config.assets must be defined");for(const o of e.assets)K(o),q[o.behavior]&&N.push(o)}();const e=uo(),o=await Promise.all(e);return globalThis.name="em-pthread",await fo(o),ke.config.exitOnUnhandledError&&He(),Ge(),Me&&ke.config.forwardConsole&&void 0!==globalThis.WebSocket&&x("main",globalThis.console,globalThis.location.origin),await S(ze),await oe(),self.dispatchEvent(new MessageEvent("message",{data:{cmd:1,handlers:ze.handlers,wasmMemory:ze.wasmMemory,wasmModule:ze.wasmModule}})),ze}().catch(e=>Xe(1,e));const mo=new class{withModuleConfig(e){try{return ve(ze,e),this}catch(e){throw Xe(1,e),e}}withInterpreterPgo(e,o){try{return ye(Ne,{interpreterPgo:e,interpreterPgoSaveDelay:o}),Ne.runtimeOptions?Ne.runtimeOptions.push("--interp-pgo-recording"):Ne.runtimeOptions=["--interp-pgo-recording"],this}catch(e){throw Xe(1,e),e}}withConfig(e){try{return ye(Ne,e),this}catch(e){throw Xe(1,e),e}}withConfigSrc(e){return this}withVirtualWorkingDirectory(e){try{return e&&"string"==typeof e||We(!1,"must be directory path"),ye(Ne,{virtualWorkingDirectory:e}),this}catch(e){throw Xe(1,e),e}}withEnvironmentVariable(e,o){try{const t={};return t[e]=o,ye(Ne,{environmentVariables:t}),this}catch(e){throw Xe(1,e),e}}withEnvironmentVariables(e){try{return e&&"object"==typeof e||We(!1,"must be dictionary object"),ye(Ne,{environmentVariables:e}),this}catch(e){throw Xe(1,e),e}}withDiagnosticTracing(e){try{return"boolean"!=typeof e&&We(!1,"must be boolean"),ye(Ne,{diagnosticTracing:e}),this}catch(e){throw Xe(1,e),e}}withDebugging(e){try{return null!=e&&"number"==typeof e||We(!1,"must be number"),ye(Ne,{debugLevel:e}),this}catch(e){throw Xe(1,e),e}}withApplicationArguments(...e){try{return e&&Array.isArray(e)||We(!1,"must be array of strings"),ye(Ne,{applicationArguments:e}),this}catch(e){throw Xe(1,e),e}}withRuntimeOptions(e){try{return e&&Array.isArray(e)||We(!1,"must be array of strings"),Ne.runtimeOptions?Ne.runtimeOptions.push(...e):Ne.runtimeOptions=e,this}catch(e){throw Xe(1,e),e}}withMainAssembly(e){try{return ye(Ne,{mainAssemblyName:e}),this}catch(e){throw Xe(1,e),e}}withApplicationArgumentsFromQuery(){try{if(!globalThis.window)throw new Error("Missing window to the query parameters from");if(void 0===globalThis.URLSearchParams)throw new Error("URLSearchParams is supported");const e=new URLSearchParams(globalThis.window.location.search).getAll("arg");return this.withApplicationArguments(...e)}catch(e){throw Xe(1,e),e}}withApplicationEnvironment(e){try{return ye(Ne,{applicationEnvironment:e}),this}catch(e){throw Xe(1,e),e}}withApplicationCulture(e){try{return ye(Ne,{applicationCulture:e}),this}catch(e){throw Xe(1,e),e}}withResourceLoader(e){try{return ke.loadBootResource=e,this}catch(e){throw Xe(1,e),e}}async download(){try{await async function(){lo(ze),await Te(ze),ne(),D(),oe(),await ke.allDownloadsFinished.promise}()}catch(e){throw Xe(1,e),e}}async create(){try{return this.instance||(this.instance=await async function(){return await co(ze),Fe.api}()),this.instance}catch(e){throw Xe(1,e),e}}run(){return this.runMainAndExit()}async runMainAndExit(){try{return ze.config||We(!1,"Null moduleConfig.config"),this.instance||await this.create(),this.instance.runMainAndExit()}catch(e){throw Xe(1,e),e}}async runMain(){try{return ze.config||We(!1,"Null moduleConfig.config"),this.instance||await this.create(),this.instance.runMain()}catch(e){throw Xe(1,e),e}}},go=Xe,po=co;Se||"function"==typeof globalThis.URL||We(!1,"This browser/engine doesn't support URL API. Please use a modern version."),"function"!=typeof globalThis.BigInt64Array&&We(!1,"This browser/engine doesn't support BigInt64Array API. Please use a modern version. See also https://learn.microsoft.com/aspnet/core/blazor/supported-platforms"),globalThis.performance&&"function"==typeof globalThis.performance.now||We(!1,"This browser/engine doesn't support performance.now. Please use a modern version."),Se||globalThis.crypto&&"object"==typeof globalThis.crypto.subtle||We(!1,"This engine doesn't support crypto.subtle. Please use a modern version."),Se||globalThis.crypto&&"function"==typeof globalThis.crypto.getRandomValues||We(!1,"This engine doesn't support crypto.getRandomValues. Please use a modern version."),Ee&&"function"!=typeof process.exit&&We(!1,"This engine doesn't support process.exit. Please use a modern version."),mo.withConfig(/*json-start*/{
  "mainAssemblyName": "Karata.Web",
  "resources": {
    "hash": "sha256-UW6yjVy9ZKIGyAvvtH3YuN0MOsFWGiLBFQqt7SW+duM=",
    "jsModuleNative": [
      {
        "name": "dotnet.native.26k62u5ri9.js"
      }
    ],
    "jsModuleRuntime": [
      {
        "name": "dotnet.runtime.uyfhwbfd13.js"
      }
    ],
    "wasmNative": [
      {
        "name": "dotnet.native.zpi6eyufzo.wasm",
        "hash": "sha256-R9ohrPekP38mgmX4B8mYg2HrVRXhU7XNweIWLlfSYeQ=",
        "cache": "force-cache"
      }
    ],
    "icu": [
      {
        "virtualPath": "icudt_CJK.dat",
        "name": "icudt_CJK.5lgyv9xn0b.dat",
        "hash": "sha256-eZuX0pntrUwNrAmFCMwpxJjFA3/Myi/rW2x9mEZ+Mbg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "icudt_EFIGS.dat",
        "name": "icudt_EFIGS.xyuimhy3ww.dat",
        "hash": "sha256-SQcxb+bdx2UXUCU9tFdOWCr4Ctk64xghCnr0JGLWWKQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "icudt_no_CJK.dat",
        "name": "icudt_no_CJK.h0en30vv0c.dat",
        "hash": "sha256-T8YllylpxyWp9Aq4AiF+BMAxKXqYyzWB9RA5RqY19vs=",
        "cache": "force-cache"
      }
    ],
    "coreAssembly": [
      {
        "payloadSize": 45136,
        "virtualPath": "System.Runtime.InteropServices.JavaScript.wasm",
        "name": "System.Runtime.InteropServices.JavaScript.vfjk7aherr.wasm",
        "hash": "sha256-a/7qJOWn20tCSpNZEEDDWgNNm1d+r6jed44yP9qZsfA=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 2376784,
        "virtualPath": "System.Private.CoreLib.wasm",
        "name": "System.Private.CoreLib.oj2blxwbph.wasm",
        "hash": "sha256-XAXTA8t1dCvSAzox5UNSdwJk6zRzyHQg2wA50jbi0kg=",
        "cache": "force-cache"
      }
    ],
    "assembly": [
      {
        "payloadSize": 11856,
        "virtualPath": "blazor-dom-confetti.wasm",
        "name": "blazor-dom-confetti.bbgyf2w3uq.wasm",
        "hash": "sha256-F/7AixYBqHhPRuN2FD4ZQ+l2JLnchWgkYHknhPNalmM=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 31824,
        "virtualPath": "Blazored.LocalStorage.wasm",
        "name": "Blazored.LocalStorage.e6jo5xh325.wasm",
        "hash": "sha256-FpAhVpaw9UJbej/kZuE9tEhHu6MMEafaUZ/Hv8zf8nk=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 11856,
        "virtualPath": "CodeBeam.MudBlazor.Extensions.wasm",
        "name": "CodeBeam.MudBlazor.Extensions.2y1nd3hd0u.wasm",
        "hash": "sha256-Zi2hAfvBRdyUtUZ46X+Owlw6D756hOPb6GnhrUhYSns=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 20048,
        "virtualPath": "Howler.Blazor.wasm",
        "name": "Howler.Blazor.58da534195.wasm",
        "hash": "sha256-iw0hbpy16Te9wEvvz2ONZY3sk0OHrCjmGX2N2TJsS7s=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 397904,
        "virtualPath": "Humanizer.wasm",
        "name": "Humanizer.wosj5f8jcq.wasm",
        "hash": "sha256-ABlJfDvyMn1dSFSK8+8wSf2lfDWSTSTSKFiNcWAue/s=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 32848,
        "virtualPath": "Microsoft.AspNetCore.Authorization.wasm",
        "name": "Microsoft.AspNetCore.Authorization.fdtxgp5zef.wasm",
        "hash": "sha256-Dcn7DGTqkvbKARUWDIgb7hogvNekZKr9bS7jsjKOg94=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 300112,
        "virtualPath": "Microsoft.AspNetCore.Components.wasm",
        "name": "Microsoft.AspNetCore.Components.0dlsxxxhu0.wasm",
        "hash": "sha256-3G/9J05YJYO+RZolDmFrFyaYJOEO2hEYOCdM/szXUOo=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 19536,
        "virtualPath": "Microsoft.AspNetCore.Components.Authorization.wasm",
        "name": "Microsoft.AspNetCore.Components.Authorization.1f5jy8x5cr.wasm",
        "hash": "sha256-LXQaeDyf99lAUEvrqNhKurGXZQMLdhKQfrEhJdMXKRk=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 19536,
        "virtualPath": "Microsoft.AspNetCore.Components.Forms.wasm",
        "name": "Microsoft.AspNetCore.Components.Forms.pgpsytsn22.wasm",
        "hash": "sha256-6eWn6iO0YyQzkm40R3wQKg5yhCAs3BYMlxVC2n34tDk=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 67664,
        "virtualPath": "Microsoft.AspNetCore.Components.Web.wasm",
        "name": "Microsoft.AspNetCore.Components.Web.2jiuwhklih.wasm",
        "hash": "sha256-Tj0NCDdlcjlYKYFiStsilQKQYlfUr91xIviWBP02Q8A=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 141392,
        "virtualPath": "Microsoft.AspNetCore.Components.WebAssembly.wasm",
        "name": "Microsoft.AspNetCore.Components.WebAssembly.4z4ovdop7r.wasm",
        "hash": "sha256-0D1JifNF4T2E93uGPEvhT/hoqsKYvMQbR0IWKwbUjvI=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 80464,
        "virtualPath": "Microsoft.AspNetCore.Components.WebAssembly.Authentication.wasm",
        "name": "Microsoft.AspNetCore.Components.WebAssembly.Authentication.nocqr6fsdc.wasm",
        "hash": "sha256-SXqbSEAMEb3q0/8LOxOqQbZhxZbwnPZgG3QOfICzQXI=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 11344,
        "virtualPath": "Microsoft.AspNetCore.Connections.Abstractions.wasm",
        "name": "Microsoft.AspNetCore.Connections.Abstractions.6vkswli1gn.wasm",
        "hash": "sha256-okwhH15n+vB8WiZqEuOEFehkzxk+ZRwToH7LA7sb4r8=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 106576,
        "virtualPath": "Microsoft.AspNetCore.Http.Connections.Client.wasm",
        "name": "Microsoft.AspNetCore.Http.Connections.Client.nrm0xkpcl8.wasm",
        "hash": "sha256-vWvU5M4BZG/YGUCQ9WXEywMZOFO823UYCtKXSVHHlyE=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 13392,
        "virtualPath": "Microsoft.AspNetCore.Http.Connections.Common.wasm",
        "name": "Microsoft.AspNetCore.Http.Connections.Common.dihosqbmbm.wasm",
        "hash": "sha256-peJV1egeipgLfdPn8pcITEfKuh/LKwAMlIspr8PS7g0=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 5200,
        "virtualPath": "Microsoft.AspNetCore.Metadata.wasm",
        "name": "Microsoft.AspNetCore.Metadata.95a9rwajxu.wasm",
        "hash": "sha256-nXYtUumA6cEasFj84Rdf9JX49pa3Md2QTyF99TfQvqI=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 8784,
        "virtualPath": "Microsoft.AspNetCore.SignalR.Client.wasm",
        "name": "Microsoft.AspNetCore.SignalR.Client.hin3pyuw9z.wasm",
        "hash": "sha256-NwUyJhZyPtOodlHpjwJCn/rCXAZcOzp5n1UhCkFur3g=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 164944,
        "virtualPath": "Microsoft.AspNetCore.SignalR.Client.Core.wasm",
        "name": "Microsoft.AspNetCore.SignalR.Client.Core.gf0uz4qdzl.wasm",
        "hash": "sha256-rNA4But4e30t1ezhtPcRbvnYJmlx3ouoZnnlbI7cyq0=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 23632,
        "virtualPath": "Microsoft.AspNetCore.SignalR.Common.wasm",
        "name": "Microsoft.AspNetCore.SignalR.Common.pnoo9ulm9m.wasm",
        "hash": "sha256-K7xRu3xKBUbvFo+eTrr7tm+rasxeiIoGLAQ9eDLjvXk=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 22608,
        "virtualPath": "Microsoft.AspNetCore.SignalR.Protocols.Json.wasm",
        "name": "Microsoft.AspNetCore.SignalR.Protocols.Json.2j0mmyqj7y.wasm",
        "hash": "sha256-0lWt04gmCvudf/TeLjTgXIy3jdhJsjVw1E0dQqBLTs0=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 8784,
        "virtualPath": "Microsoft.Extensions.Configuration.wasm",
        "name": "Microsoft.Extensions.Configuration.e6rfa9mvfw.wasm",
        "hash": "sha256-qprAZs11QknnNWs31jrjGmWxdUsXL6dZqWOH/WHzx8s=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 12880,
        "virtualPath": "Microsoft.Extensions.Configuration.EnvironmentVariables.wasm",
        "name": "Microsoft.Extensions.Configuration.EnvironmentVariables.vaqacjgd1g.wasm",
        "hash": "sha256-WWSvU1JDlu+bkUC9QASXcgI6FBxn6nqhoi+goYlADAM=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 9808,
        "virtualPath": "Microsoft.Extensions.Configuration.Json.wasm",
        "name": "Microsoft.Extensions.Configuration.Json.q81lkglnox.wasm",
        "hash": "sha256-it2ZhBMdA01nWq87yJoqpIh2zzD4VlW5qVKw+RKqPzw=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 54864,
        "virtualPath": "Microsoft.Extensions.DependencyInjection.wasm",
        "name": "Microsoft.Extensions.DependencyInjection.htc74pxz47.wasm",
        "hash": "sha256-W2vGt8MB6QHx7wCd5kqpdaOU1h7mXWDjCA2f6axHQLY=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 14928,
        "virtualPath": "Microsoft.Extensions.Diagnostics.wasm",
        "name": "Microsoft.Extensions.Diagnostics.es1aeztafo.wasm",
        "hash": "sha256-jrG0mdpst0h6GpNSYnKL6CGIpoqljOAai75Nx5vueow=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 10320,
        "virtualPath": "Microsoft.Extensions.Features.wasm",
        "name": "Microsoft.Extensions.Features.ud1s3641rx.wasm",
        "hash": "sha256-/s4l48/hJ8WLhxvN2wKGGMOIeu4O1S8T0m9Jnlo4rgk=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 21584,
        "virtualPath": "Microsoft.Extensions.Localization.wasm",
        "name": "Microsoft.Extensions.Localization.38nzfjxcrh.wasm",
        "hash": "sha256-UwtfUqtUsqs69oishFBQRYhYsK8xmEBPG3yrBs33LAs=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 7248,
        "virtualPath": "Microsoft.Extensions.Localization.Abstractions.wasm",
        "name": "Microsoft.Extensions.Localization.Abstractions.f9vrzbwx8o.wasm",
        "hash": "sha256-/dllKCTpibHQVq7lr5VSgBESN5tjz40LPS1heJo9xRw=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 19536,
        "virtualPath": "Microsoft.Extensions.Logging.wasm",
        "name": "Microsoft.Extensions.Logging.xpk1ev67m1.wasm",
        "hash": "sha256-2xbW12ofAh3V+8qjEjO/gQTbvTKG7/woocum9yc1/xU=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 47696,
        "virtualPath": "Microsoft.JSInterop.wasm",
        "name": "Microsoft.JSInterop.x07x55u912.wasm",
        "hash": "sha256-L+XFg0Ege3T53gM82kSYWMjBOAcR0kFl7pZPql5Lzfo=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 11856,
        "virtualPath": "Microsoft.JSInterop.WebAssembly.wasm",
        "name": "Microsoft.JSInterop.WebAssembly.kthmj8ue8z.wasm",
        "hash": "sha256-uHbb5b7vnE0GwS6Qws3ipDRp2E0UWNZ9G2JJrz8Au4U=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 1305168,
        "virtualPath": "MudBlazor.wasm",
        "name": "MudBlazor.y7nrk3wvej.wasm",
        "hash": "sha256-Tu5JvWN4HvRqT/Yks2SCkwq8WSYmOAAkllCyj341T90=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 74320,
        "virtualPath": "QRCoder.wasm",
        "name": "QRCoder.k1xek6mtle.wasm",
        "hash": "sha256-OFUfRDeZltaiVduMDIPLrrBvBUOoYkXCqFpXRfqOXag=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 256592,
        "virtualPath": "RestSharp.wasm",
        "name": "RestSharp.7q4fcopxdj.wasm",
        "hash": "sha256-r4EFoqqxhhxBOGRCEhDD+sRrKHNzCfq7bWHC8R/N16A=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 11856,
        "virtualPath": "Stef.Validation.wasm",
        "name": "Stef.Validation.wjntrx737d.wasm",
        "hash": "sha256-hJRYQ4Z1Q0/TWO/F1g+D9f8cjKF4qhppuTwU6MrpI9Y=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 67664,
        "virtualPath": "System.Reactive.wasm",
        "name": "System.Reactive.y5np99so8h.wasm",
        "hash": "sha256-hm6AP7/nuZQdqGmQEQ+YRDzJrR4euO2Az1Zzr63OASk=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 30288,
        "virtualPath": "TextCopy.wasm",
        "name": "TextCopy.q0ckvhnctw.wasm",
        "hash": "sha256-2kzQsf63XRl2DgRzSkDLFJNXNGtFDNcTfjk43KyQXlc=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 6224,
        "virtualPath": "Microsoft.Extensions.Configuration.Abstractions.wasm",
        "name": "Microsoft.Extensions.Configuration.Abstractions.gol3euaj3d.wasm",
        "hash": "sha256-gPYEp70ZmrxYlpLQw7hcxdmIbQ6vmluxLP8cfyoVWzo=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 27728,
        "virtualPath": "Microsoft.Extensions.DependencyInjection.Abstractions.wasm",
        "name": "Microsoft.Extensions.DependencyInjection.Abstractions.s68p0kmxtt.wasm",
        "hash": "sha256-TJrWHvY7UACzQmVk6i5Xr3bx8W1QaVfn51m/yqjW4fI=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 8272,
        "virtualPath": "Microsoft.Extensions.Diagnostics.Abstractions.wasm",
        "name": "Microsoft.Extensions.Diagnostics.Abstractions.qz20gv1zfo.wasm",
        "hash": "sha256-2jQ7jiew9q+5Ke8i4fl+RClN1kk9u0ejlFTCHyIWg4Q=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 5200,
        "virtualPath": "Microsoft.Extensions.Hosting.Abstractions.wasm",
        "name": "Microsoft.Extensions.Hosting.Abstractions.wgrd77yf8o.wasm",
        "hash": "sha256-UI/VpVCSQq1k6851I2nIRybFHWT0BJkz8pnAhtB+6fs=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 32336,
        "virtualPath": "Microsoft.Extensions.Logging.Abstractions.wasm",
        "name": "Microsoft.Extensions.Logging.Abstractions.fnx1nvsygs.wasm",
        "hash": "sha256-m/Mi1mLUt8R45BZzGE/fkq86u65u4tNXrcf+GBl1kQE=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 28240,
        "virtualPath": "Microsoft.Extensions.Options.wasm",
        "name": "Microsoft.Extensions.Options.iepn2mr15z.wasm",
        "hash": "sha256-ptU7jdDL6v+szkvLCU1Ut6scvf6UpvtTp+pEmibwCsw=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 11344,
        "virtualPath": "Microsoft.Extensions.Primitives.wasm",
        "name": "Microsoft.Extensions.Primitives.1cw8f4prw0.wasm",
        "hash": "sha256-T1vljupCAO//4Au7oNib235dVE8HSLpLlZVTjVV5Gtg=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 4688,
        "virtualPath": "Microsoft.Win32.Primitives.wasm",
        "name": "Microsoft.Win32.Primitives.rcowchsmmt.wasm",
        "hash": "sha256-S/OIwELo2q5Zqigdz1mY/Db5THZ38NHnkEsYrJ46rQg=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 5712,
        "virtualPath": "System.Collections.Concurrent.wasm",
        "name": "System.Collections.Concurrent.y1gijyulzg.wasm",
        "hash": "sha256-xvx/w4IwzVNuFAbIBp7weEiiHjSGMMhVObIV7+P9bQ4=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 71248,
        "virtualPath": "System.Collections.Immutable.wasm",
        "name": "System.Collections.Immutable.q84otin33s.wasm",
        "hash": "sha256-eiBAdRAsxbUYAWGfgDHrj97WMCHowrX1+SOpcbh5/dc=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 14928,
        "virtualPath": "System.Collections.NonGeneric.wasm",
        "name": "System.Collections.NonGeneric.zrhi1atxpt.wasm",
        "hash": "sha256-f0PZBw7C94N7PJny8JflCItCjcu3+gAkLF51YC58YP0=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 14928,
        "virtualPath": "System.Collections.Specialized.wasm",
        "name": "System.Collections.Specialized.rxg50vdqe9.wasm",
        "hash": "sha256-Ae+pcixjXaf+zSVB177gZ7Zl97pa8Ob43as5Uv+cVMk=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 28240,
        "virtualPath": "System.Collections.wasm",
        "name": "System.Collections.5kubm2vmdq.wasm",
        "hash": "sha256-iwKNs/zoj4vjP/iRzkXk2EzIa7rIFPOX1GB24oyonU4=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 6736,
        "virtualPath": "System.ComponentModel.Annotations.wasm",
        "name": "System.ComponentModel.Annotations.3z9tm0pxm7.wasm",
        "hash": "sha256-v31J6jEhZcQ/Z4uBi78jrhqzst/MOz6ZSxDMz5C0s/Q=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 11344,
        "virtualPath": "System.ComponentModel.Primitives.wasm",
        "name": "System.ComponentModel.Primitives.0x6vcw9zq5.wasm",
        "hash": "sha256-PG8JZ1ciIEmb40QZeDowASfJeJnJbGVsB53zxmsz8ew=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 58960,
        "virtualPath": "System.ComponentModel.TypeConverter.wasm",
        "name": "System.ComponentModel.TypeConverter.mjwngr5mpi.wasm",
        "hash": "sha256-fs9fCGUKFij8KamsCznQGJCNHQVxsaAVrqhKsv7I0Mk=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 4688,
        "virtualPath": "System.ComponentModel.wasm",
        "name": "System.ComponentModel.dre8ytm0cg.wasm",
        "hash": "sha256-uVJJUT1DKu5RCowE7r2KWzwN9Q9Ep0+XYi0dg7lEvvs=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 14416,
        "virtualPath": "System.Console.wasm",
        "name": "System.Console.czrwbj5s06.wasm",
        "hash": "sha256-WENStYcBpfawoMW3cFPzdoKW/YTn7i9hsTodAtGXWNc=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 154704,
        "virtualPath": "System.Diagnostics.DiagnosticSource.wasm",
        "name": "System.Diagnostics.DiagnosticSource.4b9d5du9jv.wasm",
        "hash": "sha256-oPZY7/4iuom69SE/Tu7LN7bpjsPnO+51fLwKEbp1ugs=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 7248,
        "virtualPath": "System.Diagnostics.Process.wasm",
        "name": "System.Diagnostics.Process.r1z1n44mmb.wasm",
        "hash": "sha256-UdGOI2GvRHPPvFZ9lVnJEPJSRSkdrzxb3Up5E/1PR2c=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 9808,
        "virtualPath": "System.Diagnostics.TraceSource.wasm",
        "name": "System.Diagnostics.TraceSource.f0jmvj3wrc.wasm",
        "hash": "sha256-QRvbM8YKc9r02Q0foYbZKRLYQgkvXnVYby1ipIzvUfE=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 6224,
        "virtualPath": "System.Drawing.Primitives.wasm",
        "name": "System.Drawing.Primitives.3lfgxcdysd.wasm",
        "hash": "sha256-53OhPdSfnItKytJWNVNWpVIJZdRjS+PLMiAnFKFLOd4=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 31312,
        "virtualPath": "System.IO.Compression.wasm",
        "name": "System.IO.Compression.tz6pdodhfk.wasm",
        "hash": "sha256-qYzzA4grtGM/eS+x2sXkKDsqqSyQte3Lcp2qOnBv484=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 31312,
        "virtualPath": "System.IO.Pipelines.wasm",
        "name": "System.IO.Pipelines.ib1v7e8rt7.wasm",
        "hash": "sha256-kkg/s5H1hirWy8PbVSoMh6Em4V043ax9kwPqeD6ZAEM=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 357456,
        "virtualPath": "System.Linq.Expressions.wasm",
        "name": "System.Linq.Expressions.rx4j5q3od3.wasm",
        "hash": "sha256-QWPBOoibZInkonYl9NSQIQHtwgaJs8QivfbZDwj5S9I=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 90704,
        "virtualPath": "System.Linq.wasm",
        "name": "System.Linq.sr25rxzlgr.wasm",
        "hash": "sha256-SCy9qbtiYE8qnbRUICerujnVGCkGyTJnvR79isJQshs=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 18512,
        "virtualPath": "System.Memory.wasm",
        "name": "System.Memory.4u5cgsnh30.wasm",
        "hash": "sha256-dKcD1LHBZL30DmlGDUw501v0txzuBXp3E0odFea6RTY=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 204368,
        "virtualPath": "System.Net.Http.wasm",
        "name": "System.Net.Http.adhp18477b.wasm",
        "hash": "sha256-0hp8vLyH5jaE+fvrT9CqS0J18sRQO89zE8b8xouB1CU=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 41552,
        "virtualPath": "System.Net.Primitives.wasm",
        "name": "System.Net.Primitives.dhlp5lejv4.wasm",
        "hash": "sha256-/43tOqbCTT2tIPVoJUP3nne9HWZa+H9uPapcF6of/F4=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 5200,
        "virtualPath": "System.Net.Requests.wasm",
        "name": "System.Net.Requests.0cm6e9i3sh.wasm",
        "hash": "sha256-cNcaT8RaF2avDTQKtqxYrjwhu0sW0Hv2tmwaHhhH7Nc=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 5712,
        "virtualPath": "System.Net.Security.wasm",
        "name": "System.Net.Security.xuyt9yfoug.wasm",
        "hash": "sha256-ubZLvRyrh1xKG8LRdNvkeOGaZua4+2tf32MFPCQvG0k=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 18000,
        "virtualPath": "System.Net.ServerSentEvents.wasm",
        "name": "System.Net.ServerSentEvents.qz4vs2ziox.wasm",
        "hash": "sha256-+kDLKYxW2VvU0zUkv2tJtNsAUI82g/OkeeIJyGmKnZ0=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 28752,
        "virtualPath": "System.Net.WebSockets.Client.wasm",
        "name": "System.Net.WebSockets.Client.k4f2118uaw.wasm",
        "hash": "sha256-FP5TSiJ6uhgtKCzABf6NASLlUp5bzFNitX4eMB+VvI0=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 13392,
        "virtualPath": "System.Net.WebSockets.wasm",
        "name": "System.Net.WebSockets.g3pj5p5i7i.wasm",
        "hash": "sha256-zP4y2Mk4eR5p/Yjvx5zLnvoXuxDz6xeIVg9nJpkUFgA=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 5712,
        "virtualPath": "System.ObjectModel.wasm",
        "name": "System.ObjectModel.8xnlomcc3s.wasm",
        "hash": "sha256-XFzzzRp9ij0Kqq+MBENi9pkvcOY4dDnZkpLTJRMxXfM=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 72784,
        "virtualPath": "System.Private.Uri.wasm",
        "name": "System.Private.Uri.pm2i4uqroa.wasm",
        "hash": "sha256-Lwb1ob0VrRG5UEvHOLpxJrM9aU26BNs9e0BBFrye4+o=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 36432,
        "virtualPath": "System.Private.Xml.Linq.wasm",
        "name": "System.Private.Xml.Linq.l7lwciz7uo.wasm",
        "hash": "sha256-YTebWXyosgkEd+KtSp1dzxGmKq56oiX58BSHFS9a29Q=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 1165392,
        "virtualPath": "System.Private.Xml.wasm",
        "name": "System.Private.Xml.34d0wxiwpj.wasm",
        "hash": "sha256-Y6YPDxIOP7pXGg99ksNGKG7SwlGZjnzqS/VqanP4rqQ=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 4688,
        "virtualPath": "System.Runtime.InteropServices.RuntimeInformation.wasm",
        "name": "System.Runtime.InteropServices.RuntimeInformation.s4lfg6ytlz.wasm",
        "hash": "sha256-RJ/MMQ6Nm3WSOah+LLmpwk4bz3sgAOG6szhhAmhm784=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 4688,
        "virtualPath": "System.Runtime.InteropServices.wasm",
        "name": "System.Runtime.InteropServices.cv68xvl8r2.wasm",
        "hash": "sha256-OD6ha3wB2TDsSxcehzNRZAmvBggVQ6Fyd8rkh3oOHx0=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 110160,
        "virtualPath": "System.Runtime.Numerics.wasm",
        "name": "System.Runtime.Numerics.9p3wfuj7ol.wasm",
        "hash": "sha256-Q46bf57WJteFjNeYqCFkEam1pSgs3Q1HPYx8n4yzGc4=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 11856,
        "virtualPath": "System.Runtime.wasm",
        "name": "System.Runtime.fxhddyiy1p.wasm",
        "hash": "sha256-AzHJaJxNF3Mlw1Mp/wNOFChIczLbl3D4WMRqT7NX5D4=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 11856,
        "virtualPath": "System.Security.Claims.wasm",
        "name": "System.Security.Claims.zafo57z2jp.wasm",
        "hash": "sha256-e7QumN1fuwt/9FP7i0UMgP2TmSJXe9mq7xyFVZDd1NU=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 34384,
        "virtualPath": "System.Security.Cryptography.wasm",
        "name": "System.Security.Cryptography.ngi2aad2mh.wasm",
        "hash": "sha256-98umva7UUQtO3eiF+k1gi3Yei9JhrSaP/0eZ0SjtjwM=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 32336,
        "virtualPath": "System.Text.Encodings.Web.wasm",
        "name": "System.Text.Encodings.Web.qfc15pkwq7.wasm",
        "hash": "sha256-Nl62YRrTet0DvT6UF7hW8V3X3RYIUpPPyTu9ex3qNQY=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 461904,
        "virtualPath": "System.Text.Json.wasm",
        "name": "System.Text.Json.ku2c8yj1qh.wasm",
        "hash": "sha256-lCbWG7LRTvfqx/GNMpIBjhVA6efIFXntiu8f6Nr+k5A=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 166992,
        "virtualPath": "System.Text.RegularExpressions.wasm",
        "name": "System.Text.RegularExpressions.tn4ww0p4g1.wasm",
        "hash": "sha256-mY/gWFAhyZdRoMT7fkAYV2GiTc/2twHE5hfT9aYNmOY=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 35408,
        "virtualPath": "System.Threading.Channels.wasm",
        "name": "System.Threading.Channels.a7kkpv1kep.wasm",
        "hash": "sha256-ure4lBzdSH6LsfZPWp9DhgRChvH3MynufrmkFAh+ahc=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 4688,
        "virtualPath": "System.Threading.Tasks.Extensions.wasm",
        "name": "System.Threading.Tasks.Extensions.e3bga0wkqa.wasm",
        "hash": "sha256-1hkjuJH8rRK3UAH2wjZneMXn4YCBGleDTSVLr46/IWg=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 4688,
        "virtualPath": "System.Threading.Thread.wasm",
        "name": "System.Threading.Thread.62hau9nzd1.wasm",
        "hash": "sha256-3kbGowlf9DdvEI5Ea3qYxUcCwnnbNHY+wSgVlfRvkBk=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 4688,
        "virtualPath": "System.Threading.wasm",
        "name": "System.Threading.soiclj5fk1.wasm",
        "hash": "sha256-nFo2pRcua7glr5nwBlD3kdGbsNuXITvF35FCwnMAt7A=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 10832,
        "virtualPath": "System.Web.HttpUtility.wasm",
        "name": "System.Web.HttpUtility.su1vqtdm5l.wasm",
        "hash": "sha256-3KYk4rIHXbvesv9LdtMJGmEmIv5LskC/5wBBYctqDyE=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 4176,
        "virtualPath": "System.Xml.Linq.wasm",
        "name": "System.Xml.Linq.6jrwceho2h.wasm",
        "hash": "sha256-Yue7a3j8L0A300dI+FSbE9aWjzz0C7n1n3cYfwSb/as=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 4688,
        "virtualPath": "System.Xml.ReaderWriter.wasm",
        "name": "System.Xml.ReaderWriter.nb9bsyyzky.wasm",
        "hash": "sha256-80fu2G25SZV9QfbeAI4XpBoKSB+7c9QLkFSVDvnTzXY=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 4688,
        "virtualPath": "System.Xml.XmlSerializer.wasm",
        "name": "System.Xml.XmlSerializer.ta9zdz8typ.wasm",
        "hash": "sha256-K41p3F23/bTsAv6sy0956oyYzrnuv0YhbEBFrOOfEF8=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 4688,
        "virtualPath": "System.wasm",
        "name": "System.n1lgwvdeih.wasm",
        "hash": "sha256-deV0DUTbY0DlBTfQncbI/Sg+Eb7dON2nXbyolKeyvTM=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 5712,
        "virtualPath": "netstandard.wasm",
        "name": "netstandard.lawkg2a0i2.wasm",
        "hash": "sha256-fdiOIL39Hdsoiel3xyeYzxiydQwEW/ix9Cvjk3gJDG8=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 165456,
        "virtualPath": "Karata.Kit.wasm",
        "name": "Karata.Kit.7s9ef56qm3.wasm",
        "hash": "sha256-B6xuT/yepcl73p8UCQ6qeFI5YOalHkKQ4+e4QMAS84M=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 12368,
        "virtualPath": "Karata.Pebble.wasm",
        "name": "Karata.Pebble.rogktupbo2.wasm",
        "hash": "sha256-e1a60tkIV7gA0QC5YI9y9Mzl5Kl8o+L5VJDaLlfvzOA=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 10832,
        "virtualPath": "Karata.Pips.wasm",
        "name": "Karata.Pips.gdfodqlp5r.wasm",
        "hash": "sha256-1MgkeaBMJNZBbobXwsYCwq/bKfL0wDrlqM8XWxIQ7LY=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 229968,
        "virtualPath": "Karata.Surface.wasm",
        "name": "Karata.Surface.yvq9sbm5t5.wasm",
        "hash": "sha256-oscCj8hjN+L8arQhoSZNwo94E+SzMxWnR3SMUdFY+eQ=",
        "cache": "force-cache"
      },
      {
        "payloadSize": 216144,
        "virtualPath": "Karata.Web.wasm",
        "name": "Karata.Web.okcqma98sr.wasm",
        "hash": "sha256-KJBSkK0s1blGVSgxk7saaza855McqtHUobRpzMFJS88=",
        "cache": "force-cache"
      }
    ],
    "satelliteResources": {
      "af": [
        {
          "payloadSize": 7760,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.73vb5lizga.wasm",
          "hash": "sha256-I+uRq2ffxoZxfYbnsnEvswovFKUsFB5lPFXWb+wStqo=",
          "cache": "force-cache"
        }
      ],
      "ar": [
        {
          "payloadSize": 12880,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.ju5f4ioqtw.wasm",
          "hash": "sha256-pSgmsgHpRWDRaXTRnm7HVIDP9bvBToIY0HzBzu1JUu8=",
          "cache": "force-cache"
        }
      ],
      "az": [
        {
          "payloadSize": 7760,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.zesjqrxyuy.wasm",
          "hash": "sha256-m0zjuwaV8V9INRwQat9tvB7L+XBrFRXxSt6Av06XNag=",
          "cache": "force-cache"
        }
      ],
      "bg": [
        {
          "payloadSize": 9296,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.awoqyu1knp.wasm",
          "hash": "sha256-iqrwC1hsbvVM5TAc6bvtI49J0mEf1dc+2/F8gDMf9ro=",
          "cache": "force-cache"
        }
      ],
      "bn": [
        {
          "payloadSize": 8272,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.tvvz5jq9d8.wasm",
          "hash": "sha256-10LX7sjWSXfwgJ/3GLgCRG6nMyupSUeG5fKQaHjtcGg=",
          "cache": "force-cache"
        }
      ],
      "ca": [
        {
          "payloadSize": 19024,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.96ftgv0aui.wasm",
          "hash": "sha256-EV5Nh6Oym1ltkjU3MThnXF4zVHXNb3nCzAbP5B1XoBM=",
          "cache": "force-cache"
        }
      ],
      "cs": [
        {
          "payloadSize": 9808,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.q5ijv0j9a1.wasm",
          "hash": "sha256-Ui5L7/pl3++dpHgkzo6D3qg9dXgt2kLZ6go0bHzUmqw=",
          "cache": "force-cache"
        }
      ],
      "da": [
        {
          "payloadSize": 8784,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.m4du50ky4x.wasm",
          "hash": "sha256-9oJfYcOVb74w+qrfjmrEldlChY9StLWic2VUJm+Xfus=",
          "cache": "force-cache"
        }
      ],
      "de": [
        {
          "payloadSize": 10832,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.m9w543mvas.wasm",
          "hash": "sha256-msJJehnxTnUmdN+EP6aWpT505LBa9KFzJoPoEel/CvM=",
          "cache": "force-cache"
        }
      ],
      "el": [
        {
          "payloadSize": 8784,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.4p7zdrkub4.wasm",
          "hash": "sha256-SLW2MupO6hfyOxK0P3jPOCXhfApipnbgrvjcX0naFdw=",
          "cache": "force-cache"
        }
      ],
      "es": [
        {
          "payloadSize": 19024,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.jsemw82qq2.wasm",
          "hash": "sha256-Y9RLqbFZ564pNo0Stw+1ECy5tenid1chZt6TcTxx9lo=",
          "cache": "force-cache"
        }
      ],
      "fa": [
        {
          "payloadSize": 8272,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.kme61if0ui.wasm",
          "hash": "sha256-jKilu6vVWb0eH0DtBYkro9WBK7HJ7wymjjS0QwtiRrQ=",
          "cache": "force-cache"
        }
      ],
      "fi": [
        {
          "payloadSize": 6736,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.zb34yzqrwu.wasm",
          "hash": "sha256-Iejw7aRxieLqWitoUOKM2wnl0FMKd7TrIFwCmuG0zPg=",
          "cache": "force-cache"
        }
      ],
      "fil": [
        {
          "payloadSize": 15952,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.p6e15tml0d.wasm",
          "hash": "sha256-EVdgwq+vm5s/JzfcfBBXxMO7SwADsDdxhhZaJhKFW9c=",
          "cache": "force-cache"
        }
      ],
      "fr": [
        {
          "payloadSize": 10832,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.wbjtbr250f.wasm",
          "hash": "sha256-HJ6vGRtS6v9rl9afQTXmGE8IxT5LSjdrlcnSXvnoqnw=",
          "cache": "force-cache"
        }
      ],
      "he": [
        {
          "payloadSize": 12368,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.c3no0yb1ln.wasm",
          "hash": "sha256-6EIwsKHJuOMoOnZVnVo7s98stThCtMvTlECXnrciL2g=",
          "cache": "force-cache"
        }
      ],
      "hr": [
        {
          "payloadSize": 9808,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.qn7wac0mff.wasm",
          "hash": "sha256-OpfEzDZzbMamnO7HM1qI1ibLz5hAOPmpd8LfAw/QpmY=",
          "cache": "force-cache"
        }
      ],
      "hu": [
        {
          "payloadSize": 18512,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.3dtxco5zs9.wasm",
          "hash": "sha256-GfGmMCBz+7bYHsN64HwiPeaI2sg8FklsI3uLwze8ozg=",
          "cache": "force-cache"
        }
      ],
      "hy": [
        {
          "payloadSize": 8272,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.6s03rk0lsh.wasm",
          "hash": "sha256-tjGGC9XUtYlyvVTk5s9zBCGADYFB+ddKjTSfKAszK8Y=",
          "cache": "force-cache"
        }
      ],
      "id": [
        {
          "payloadSize": 8272,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.2m5tuyticv.wasm",
          "hash": "sha256-VkhjyBPuFwv372B5n572NnuV1Od/rAmvZ8tWaILs7r0=",
          "cache": "force-cache"
        }
      ],
      "is": [
        {
          "payloadSize": 14928,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.7aebgaeu1i.wasm",
          "hash": "sha256-8QrsNuNndYDw/NzOieKtxEWHElZxNEpozxF2yEyUh8o=",
          "cache": "force-cache"
        }
      ],
      "it": [
        {
          "payloadSize": 8784,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.fo68xv7znv.wasm",
          "hash": "sha256-z6CcnjSBuDcwICjDI/6NYSbFZgnTPjZ2QbeGjOUAAq8=",
          "cache": "force-cache"
        }
      ],
      "ja": [
        {
          "payloadSize": 7760,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.oygeldx5o8.wasm",
          "hash": "sha256-YCavMGXOR50gO5iG7MC/qesvMcuGv/clFeIQkX2kPtg=",
          "cache": "force-cache"
        }
      ],
      "ko": [
        {
          "payloadSize": 14928,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.oparj4zj5d.wasm",
          "hash": "sha256-/OErjG4opptm6pgccSKrcFGHIfaXxciOFObesq6nz0U=",
          "cache": "force-cache"
        }
      ],
      "ku": [
        {
          "payloadSize": 18000,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.ooupqarphd.wasm",
          "hash": "sha256-mBk3xH2v934kXylMLpP6MABS3OoNdn0kQhHJhui3Ng8=",
          "cache": "force-cache"
        }
      ],
      "lb": [
        {
          "payloadSize": 9808,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.fv9qoely0s.wasm",
          "hash": "sha256-hWtQQ3fBHprMsPToTZC0ZkLv+JoVjF1Xe3Wq6jflruI=",
          "cache": "force-cache"
        }
      ],
      "lt": [
        {
          "payloadSize": 12880,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.x9jjxbwslc.wasm",
          "hash": "sha256-VbXWBdOUFjNtYnez5j2cf1+hB+/0JUHDhUFY1NVp30U=",
          "cache": "force-cache"
        }
      ],
      "lv": [
        {
          "payloadSize": 15440,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.2kkfo35bjo.wasm",
          "hash": "sha256-0ikd9lzufrgtUi2i67XwXXY/odCVSsZEYv2iz6N4yhM=",
          "cache": "force-cache"
        }
      ],
      "ms": [
        {
          "payloadSize": 15440,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.u1vdmtmzqs.wasm",
          "hash": "sha256-jpdV7azx0N7rQS8DFTy2HmOF4YLxYzYs4QnMlL9TT3M=",
          "cache": "force-cache"
        }
      ],
      "mt": [
        {
          "payloadSize": 18000,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.737vequolh.wasm",
          "hash": "sha256-o4tUB3bP3r59dOPNMYkdvg9lnmL4Q4PfjaBdwVYshOk=",
          "cache": "force-cache"
        }
      ],
      "nb": [
        {
          "payloadSize": 8784,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.z5t6nx9j4u.wasm",
          "hash": "sha256-2TsMdYVQpY3gUrQLK8qB1eEA7Kbx9kZa+wlVtlKnqIk=",
          "cache": "force-cache"
        }
      ],
      "nl": [
        {
          "payloadSize": 7760,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.q4a672pup4.wasm",
          "hash": "sha256-yjdKPNFa9HUlpv7tHxls65d4PwO9nop218NYeYvvBmk=",
          "cache": "force-cache"
        }
      ],
      "pl": [
        {
          "payloadSize": 10320,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.277mc21smp.wasm",
          "hash": "sha256-Ek4Pe0cC/v/784RZV6a3+gWA1H9UVZfIaUNOuYcyKRs=",
          "cache": "force-cache"
        }
      ],
      "pt": [
        {
          "payloadSize": 19024,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.e4h6at00os.wasm",
          "hash": "sha256-0oXeiFefqReHtporMVrT49F2dxW+ktryJuUdRz5v6q8=",
          "cache": "force-cache"
        }
      ],
      "pt-BR": [
        {
          "payloadSize": 19024,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.6eygdgrjln.wasm",
          "hash": "sha256-4hDGUDpFiGWDciVxEb/G9asoTWu4rCd9r0A3NLl7CnU=",
          "cache": "force-cache"
        }
      ],
      "ro": [
        {
          "payloadSize": 7760,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.qz7fu9dfh0.wasm",
          "hash": "sha256-yvf5fVpeFSeoIOuB0tUO5r3nmUXL45R9ZHDP4E3+d6c=",
          "cache": "force-cache"
        }
      ],
      "ru": [
        {
          "payloadSize": 20560,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.5j9fmnyvr2.wasm",
          "hash": "sha256-/69v7nP4xFDSjmihkVnvapSX3FVCgFrCYRnGi+vuBUc=",
          "cache": "force-cache"
        }
      ],
      "sk": [
        {
          "payloadSize": 9808,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.csk47xzsk9.wasm",
          "hash": "sha256-PP1bD/ZJouIjf+TOU9vOjZftkge1L3gmQAtlgJjx620=",
          "cache": "force-cache"
        }
      ],
      "sl": [
        {
          "payloadSize": 12368,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.swvr4hiqdn.wasm",
          "hash": "sha256-TOLJ3yGN9QQcW7IUTdKEwKPYTDyVdhpZHHwXCPPE8G8=",
          "cache": "force-cache"
        }
      ],
      "sr": [
        {
          "payloadSize": 10320,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.yu9xstn5hy.wasm",
          "hash": "sha256-KpBk43ZeQMcAKZRlFwZeeuwr04fisTKbhZn4vLJn22E=",
          "cache": "force-cache"
        }
      ],
      "sr-Latn": [
        {
          "payloadSize": 9808,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.wg74jxg3ib.wasm",
          "hash": "sha256-0NUCpdjAX1A0+uth2NizIC6qg06EGXkQpedE3Z6KFIE=",
          "cache": "force-cache"
        }
      ],
      "sv": [
        {
          "payloadSize": 7760,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.1s12s63jyp.wasm",
          "hash": "sha256-gdng6n9AappyfvpyDtEL73RXZA14Sp3FGXo7VRMc58M=",
          "cache": "force-cache"
        }
      ],
      "th": [
        {
          "payloadSize": 17488,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.jv5l7e3vcy.wasm",
          "hash": "sha256-/18yGKKCYHt3BT2pvhIMYExGMzx3RlEf1kj6IKBGG2o=",
          "cache": "force-cache"
        }
      ],
      "tr": [
        {
          "payloadSize": 7760,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.shvp3s6cn0.wasm",
          "hash": "sha256-sJ6172fd3ZRaiB9XvSQYoks5vaxvo8G/CpHS8jukJwY=",
          "cache": "force-cache"
        }
      ],
      "uk": [
        {
          "payloadSize": 13904,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.p8b37bj2lh.wasm",
          "hash": "sha256-V4EBkIVWAjfdDbyxuvvVce4CJBGfdx6QtmKHlArNtv4=",
          "cache": "force-cache"
        }
      ],
      "uz-Cyrl-UZ": [
        {
          "payloadSize": 8272,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.bkv7bcly4x.wasm",
          "hash": "sha256-iwa4uyKbAiQ5rPipz7LjuL7KCU2fmvzDtB9y08PrCag=",
          "cache": "force-cache"
        }
      ],
      "uz-Latn-UZ": [
        {
          "payloadSize": 7760,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.j2nxei3upg.wasm",
          "hash": "sha256-C/n/UkCx7LxxEbYrc1gYhO0MOrQEkncVAK5Vavswn7M=",
          "cache": "force-cache"
        }
      ],
      "vi": [
        {
          "payloadSize": 8272,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.dzdv907wof.wasm",
          "hash": "sha256-d46NnpBGMxKZk7EXJ9Pf26VTPInNzvTBeeUyYvl3ZXo=",
          "cache": "force-cache"
        }
      ],
      "zh-CN": [
        {
          "payloadSize": 7760,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.noqdw01m1w.wasm",
          "hash": "sha256-qYe4FCeRwVb5HylGK3nZ72VVZmeof2B4VFU3NYFyfuE=",
          "cache": "force-cache"
        }
      ],
      "zh-Hans": [
        {
          "payloadSize": 7760,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.qt00e9xmvg.wasm",
          "hash": "sha256-0tkfng3yYkKk7iODiWTIuuIJRqKgGiJgsd62ma9mkHU=",
          "cache": "force-cache"
        }
      ],
      "zh-Hant": [
        {
          "payloadSize": 7760,
          "virtualPath": "Humanizer.resources.wasm",
          "name": "Humanizer.resources.wja5yo5rfq.wasm",
          "hash": "sha256-VGmCK/x7LcPtpIGCD+6XoPoEWLCrjUs9NPM53tWzHIg=",
          "cache": "force-cache"
        }
      ]
    }
  },
  "debugLevel": 0,
  "globalizationMode": "sharded",
  "extensions": {
    "blazor": {}
  },
  "runtimeConfig": {
    "runtimeOptions": {
      "configProperties": {
        "Microsoft.AspNetCore.Components.Routing.RegexConstraintSupport": false,
        "Microsoft.Extensions.DependencyInjection.VerifyOpenGenericServiceTrimmability": true,
        "System.ComponentModel.DefaultValueAttribute.IsSupported": false,
        "System.ComponentModel.Design.IDesignerHost.IsSupported": false,
        "System.ComponentModel.TypeConverter.EnableUnsafeBinaryFormatterInDesigntimeLicenseContextSerialization": false,
        "System.ComponentModel.TypeDescriptor.IsComObjectDescriptorSupported": false,
        "System.Data.DataSet.XmlSerializationIsSupported": false,
        "System.Diagnostics.Debugger.IsSupported": false,
        "System.Diagnostics.Metrics.Meter.IsSupported": true,
        "System.Diagnostics.Tracing.EventSource.IsSupported": true,
        "System.GC.Server": true,
        "System.Globalization.Invariant": false,
        "System.TimeZoneInfo.Invariant": false,
        "System.Linq.Enumerable.IsSizeOptimized": true,
        "System.Net.Http.EnableActivityPropagation": true,
        "System.Net.Http.WasmEnableStreamingResponse": true,
        "System.Net.SocketsHttpHandler.Http3Support": false,
        "System.Reflection.Metadata.MetadataUpdater.IsSupported": false,
        "System.Resources.ResourceManager.AllowCustomResourceTypes": false,
        "System.Resources.UseSystemResourceKeys": true,
        "System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported": true,
        "System.Runtime.InteropServices.BuiltInComInterop.IsSupported": false,
        "System.Runtime.InteropServices.EnableConsumingManagedCodeFromNativeHosting": false,
        "System.Runtime.InteropServices.EnableCppCLIHostActivation": false,
        "System.Runtime.InteropServices.Marshalling.EnableGeneratedComInterfaceComImportInterop": false,
        "System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization": false,
        "System.StartupHookProvider.IsSupported": false,
        "System.Text.Encoding.EnableUnsafeUTF7Encoding": false,
        "System.Text.Json.JsonSerializer.IsReflectionEnabledByDefault": true,
        "System.Threading.Thread.EnableAutoreleasePool": false,
        "Microsoft.AspNetCore.Components.Endpoints.NavigationManager.DisableThrowNavigationException": false,
        "System.Diagnostics.StackTrace.IsLineNumberSupported": false,
        "System.Runtime.CompilerServices.RuntimeFeature.IsMultithreadingSupported": false
      }
    }
  }
}/*json-end*/);export{po as default,mo as dotnet,go as exit};
