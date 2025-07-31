import { Redirect, Route } from 'react-router-dom';
import { IonApp, IonRouterOutlet, IonTabs, IonTabBar, IonTabButton, IonIcon, IonLabel, setupIonicReact } from '@ionic/react';
import { IonReactRouter } from '@ionic/react-router';
import { storefront, heart, person, search } from 'ionicons/icons';
import { Provider } from 'react-redux';
import { store } from './store/store';

// Pages
import Login from './pages/Login';
import Products from './pages/Products';
import ProductDetail from './pages/ProductDetail';
import Wishlist from './pages/Wishlist';
import Profile from './pages/Profile';

// CSS
import '@ionic/react/css/core.css';
import '@ionic/react/css/normalize.css';
import '@ionic/react/css/structure.css';
import '@ionic/react/css/typography.css';
import '@ionic/react/css/padding.css';
import '@ionic/react/css/float-elements.css';
import '@ionic/react/css/text-alignment.css';
import '@ionic/react/css/text-transformation.css';
import '@ionic/react/css/flex-utils.css';
import '@ionic/react/css/display.css';
import './theme/variables.css';

setupIonicReact();

const App: React.FC = () => (
  <Provider store={store}>
    <IonApp>
      <IonReactRouter>
        <IonTabs>
          <IonRouterOutlet>
            {/* Public Routes */}
            <Route exact path="/login" component={Login} />
            
            {/* Protected Routes */}
            <Route exact path="/products" component={Products} />
            <Route exact path="/products/:id" component={ProductDetail} />
            <Route exact path="/wishlist" component={Wishlist} />
            <Route exact path="/profile" component={Profile} />
            
            {/* Default redirect */}
            <Route exact path="/">
              <Redirect to="/products" />
            </Route>
          </IonRouterOutlet>

          <IonTabBar slot="bottom">
            <IonTabButton tab="products" href="/products">
              <IonIcon icon={storefront} />
              <IonLabel>Productos</IonLabel>
            </IonTabButton>

            <IonTabButton tab="search" href="/products">
              <IonIcon icon={search} />
              <IonLabel>Buscar</IonLabel>
            </IonTabButton>

            <IonTabButton tab="wishlist" href="/wishlist">
              <IonIcon icon={heart} />
              <IonLabel>Favoritos</IonLabel>
            </IonTabButton>

            <IonTabButton tab="profile" href="/profile">
              <IonIcon icon={person} />
              <IonLabel>Perfil</IonLabel>
            </IonTabButton>
          </IonTabBar>
        </IonTabs>
      </IonReactRouter>
    </IonApp>
  </Provider>
);

export default App;