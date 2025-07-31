// app.config.ts
import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideHttpClient } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient() // ✅ Agrega esta línea
  ]
};



// import { importProvidersFrom } from '@angular/core';
// import { provideRouter } from '@angular/router';
// import { HttpClientModule } from '@angular/common/http';
// import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

// import { routes } from './app.routes';

// export const appConfig = {
//   providers: [
//     importProvidersFrom(
//       HttpClientModule,
//       BrowserAnimationsModule
//     ),
//     provideRouter(routes)
//   ]
// };
