TEMPLATE = lib
CONFIG -= qt

SOURCES += \
    vsop87.c \
    venus.c \
    utility.c \
    uranus.c \
    transform.c \
    solar.c \
    sidereal_time.c \
    saturn.c \
    rise_set.c \
    refraction.c \
    proper_motion.c \
    precession.c \
    pluto.c \
    parallax.c \
    parabolic_motion.c \
    nutation.c \
    neptune.c \
    mercury.c \
    mars.c \
    lunar.c \
    jupiter.c \
    julian_day.c \
    hyperbolic_motion.c \
    heliocentric_time.c \
    elliptic_motion.c \
    earth.c \
    dynamical_time.c \
    comet.c \
    asteroid.c \
    apparent_position.c \
    angular_separation.c \
    airmass.c \
    aberration.c

HEADERS += \
    libnova/vsop87.h \
    libnova/venus.h \
    libnova/utility.h \
    libnova/uranus.h \
    libnova/transform.h \
    libnova/solar.h \
    libnova/sidereal_time.h \
    libnova/saturn.h \
    libnova/rise_set.h \
    libnova/refraction.h \
    libnova/proper_motion.h \
    libnova/precession.h \
    libnova/pluto.h \
    libnova/parallax.h \
    libnova/parabolic_motion.h \
    libnova/nutation.h \
    libnova/neptune.h \
    libnova/mercury.h \
    libnova/mars.h \
    libnova/lunar.h \
    libnova/ln_types.h \
    libnova/libnova.h \
    libnova/jupiter.h \
    libnova/julian_day.h \
    libnova/hyperbolic_motion.h \
    libnova/heliocentric_time.h \
    libnova/elliptic_motion.h \
    libnova/earth.h \
    libnova/dynamical_time.h \
    libnova/comet.h \
    libnova/asteroid.h \
    libnova/apparent_position.h \
    libnova/angular_separation.h \
    libnova/airmass.h \
    libnova/aberration.h

AM_CFLAGS = -Wall -O3 $(AVX_CFLAGS)
