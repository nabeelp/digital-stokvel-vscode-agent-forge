# Digital Stokvel Banking - Mobile App

React Native cross-platform mobile application for Digital Stokvel Banking.

## Features

- **Multi-language support**: English, isiZulu, Sesotho, Xhosa, Afrikaans
- **Group management**: Create and join stokvel groups
- **Contribution tracking**: Make and track contributions
- **Real-time ledger**: View immutable group transaction history
- **Push notifications**: Firebase Cloud Messaging for payment reminders
- **Offline support**: AsyncStorage for local data persistence

## Prerequisites

- Node.js 18+ 
- React Native 0.76+
- Android Studio (for Android development)
- Xcode (for iOS development, macOS only)
- CocoaPods (for iOS dependencies)

## Setup

```bash
# Install dependencies
npm install

# iOS only: Install CocoaPods
cd ios && pod install && cd ..

# Start Metro bundler
npm start

# Run on Android
npm run android

# Run on iOS (macOS only)
npm run ios
```

## Environment Variables

Copy `.env.example` to `.env` and configure:

```
API_BASE_URL=http://localhost:7001/api
FIREBASE_API_KEY=your_firebase_key
FIREBASE_PROJECT_ID=your_project_id
```

## Project Structure

```
src/
├── components/       # Reusable UI components
├── screens/          # App screens (Home, Groups, Contributions, etc.)
├── services/         # API clients and business logic
├── localization/     # i18n translations (5 languages)
└── navigation/       # React Navigation routes
```

## Testing

```bash
npm test
```

## Building for Production

### Android

```bash
# Generate release APK
cd android && ./gradlew assembleRelease

# Generate release AAB (for Play Store)
cd android && ./gradlew bundleRelease
```

### iOS

```bash
# Open in Xcode
open ios/DigitalStokvelMobile.xcworkspace

# Build via Xcode: Product → Archive
```

## Tech Stack

- React Native 0.76+
- TypeScript
- React Navigation 6
- TanStack Query (React Query)
- i18next
- AsyncStorage
- Firebase Cloud Messaging
- Axios

## License

Proprietary - All rights reserved.
